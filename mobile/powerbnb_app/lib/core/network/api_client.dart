import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'dart:io' show Platform;
import 'package:flutter/foundation.dart' show kIsWeb;

import '../../main.dart';
import '../../modules/auth/views/login_screen.dart';

class ApiClient {
  late Dio dio;
  final _storage = const FlutterSecureStorage();
  bool _isRefreshing = false;

  // Instância limpa para evitar que interceptores interfiram no fluxo de login/refresh
  final Dio _authDio = Dio();

  ApiClient() {
    String getBaseUrl() {
      if (kIsWeb) return 'http://localhost:5276';
      if (Platform.isAndroid) return 'http://192.168.0.71:5276';
      return 'http://localhost:5276';
    }

    dio = Dio(BaseOptions(baseUrl: getBaseUrl()));

    // Interceptor para injetar o token em todas as chamadas para o BFF
    dio.interceptors.add(
      InterceptorsWrapper(
        onRequest: (options, handler) async {
          final token = await _storage.read(key: 'jwt_token');
          if (token != null) {
            options.headers['Authorization'] = 'Bearer $token';
          }
          return handler.next(options);
        },
        onError: (error, handler) async {
          if (error.response?.statusCode == 401) {
            // O Token expirou! Vamos tentar renovar.
            final success = await _refreshToken();

            if (success) {
              // Deu certo! Pega o novo token e refaz a requisição original que havia falhado.
              final newToken = await _storage.read(key: 'jwt_token');
              final opts = error.requestOptions;
              opts.headers['Authorization'] = 'Bearer $newToken';

              try {
                // Refaz a chamada para o BFF
                final cloneReq = await dio.fetch(opts);
                return handler.resolve(
                  cloneReq,
                ); // Retorna o sucesso como se nada tivesse acontecido
              } catch (e) {
                return handler.next(error);
              }
            } else {
              // O Refresh Token também expirou. O usuário precisa fazer login de novo.
              await _storage.deleteAll();

              navigatorKey.currentState?.pushAndRemoveUntil(
                MaterialPageRoute(builder: (context) => const LoginScreen()),
                    (route) => false,
              );
            }
          }

          // Se não for 401, deixa o erro seguir o fluxo normal
          return handler.next(error);
        },
      ),
    );
  }

  Future<bool> _refreshToken() async {
    if (_isRefreshing)
      return false; // Evita concorrência se várias chamadas derem 401 ao mesmo tempo

    _isRefreshing = true;

    try {
      final refreshToken = await _storage.read(key: 'refresh_token');
      if (refreshToken == null) return false;

      final String realm = "powerbnb-app";
      final String host = Platform.isAndroid ? '10.0.2.2' : 'localhost';
      final String url =
          'http://$host:8080/realms/$realm/protocol/openid-connect/token';

      final response = await _authDio.post(
        url,
        options: Options(
          headers: {'Content-Type': 'application/x-www-form-urlencoded'},
        ),
        data: {
          'client_id': 'powerbnb-mobile',
          'grant_type': 'refresh_token',
          'refresh_token': refreshToken,
        },
      );

      if (response.statusCode == 200) {
        // Salva os novos tokens no cofre
        await _storage.write(
          key: 'jwt_token',
          value: response.data['access_token'],
        );
        await _storage.write(
          key: 'refresh_token',
          value: response.data['refresh_token'],
        );
        return true;
      }
      return false;
    } catch (e) {
      return false;
    } finally {
      _isRefreshing = false;
    }
  }

  // --- MÉTODO DE LOGIN (O QUE ESTAVA FALTANDO) ---
  Future<bool> login(String username, String password) async {
    try {
      // Ajuste conforme o seu novo Realm 'powerbnb-app'
      final String realm = "powerbnb";
      final String host = Platform.isAndroid ? '10.0.2.2' : '192.168.0.71';
      final String url =
          'http://$host:8080/realms/$realm/protocol/openid-connect/token';

      final response = await _authDio.post(
        url,
        options: Options(
          headers: {'Content-Type': 'application/x-www-form-urlencoded'},
        ),
        data: {
          'client_id': 'powerbnb-api',
          'grant_type': 'password',
          'username': username,
          'password': password,
          // 'client_secret': 'SEU_SECRET_SE_O_CLIENT_NAO_FOR_PUBLIC',
        },
      );

      if (response.statusCode == 200) {
        final accessToken = response.data['access_token'];
        final refreshToken = response.data['refresh_token'];

        await _storage.write(key: 'jwt_token', value: accessToken);
        await _storage.write(key: 'refresh_token', value: refreshToken);

        return true;
      }
      return false;
    } catch (e) {
      print("Erro no Login: $e");
      return false;
    }
  }
}
