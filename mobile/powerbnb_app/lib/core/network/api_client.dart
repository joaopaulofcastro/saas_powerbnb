import 'package:dio/dio.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'dart:io' show Platform;
import 'package:flutter/foundation.dart' show kIsWeb;

class ApiClient {
  late Dio dio;
  final _storage = const FlutterSecureStorage();
  
  // Instância limpa para evitar que interceptores interfiram no fluxo de login/refresh
  final Dio _authDio = Dio(); 

  ApiClient() {
    String getBaseUrl() {
      if (kIsWeb) return 'http://localhost:5046'; 
      if (Platform.isAndroid) return 'http://192.168.0.71:5046';
      return 'http://localhost:5046';
    }

    dio = Dio(BaseOptions(baseUrl: getBaseUrl()));

    // Interceptor para injetar o token em todas as chamadas para o BFF
    dio.interceptors.add(InterceptorsWrapper(
      onRequest: (options, handler) async {
        final token = await _storage.read(key: 'jwt_token');
        if (token != null) {
          options.headers['Authorization'] = 'Bearer $token';
        }
        return handler.next(options);
      },
      onError: (error, handler) async {
        if (error.response?.statusCode == 401) {
          // Lógica de Refresh que discutimos (omitida aqui para focar no método de login)
          // ...
        }
        return handler.next(error);
      },
    ));
  }

  // --- MÉTODO DE LOGIN (O QUE ESTAVA FALTANDO) ---
  Future<bool> login(String username, String password) async {
    try {
      // Ajuste conforme o seu novo Realm 'powerbnb-app'
      final String realm = "powerbnb";
      final String host = Platform.isAndroid ? '10.0.2.2' : '192.168.0.71';
      final String url = 'http://$host:8080/realms/$realm/protocol/openid-connect/token';

      final response = await _authDio.post(
        url,
        options: Options(
          headers: {'Content-Type': 'application/x-www-form-urlencoded'},
        ),
        data: {
          'client_id': 'powerbnb-app',
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