// lib/core/auth/auth_service.dart
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class AuthService {
  final _storage = const FlutterSecureStorage();

  // Verifica se o usuário já está logado para decidir o redirecionamento inicial
  Future<bool> isAuthenticated() async {
    final token = await _storage.read(key: 'jwt_token');
    if (token == null) return false;
    
    // Aqui no futuro você pode adicionar uma validação de expiração local do JWT
    return true;
  }

  Future<void> logout() async {
    await _storage.deleteAll();
  }
}