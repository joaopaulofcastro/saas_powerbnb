import 'package:flutter/material.dart';
import '../../../../core/network/api_client.dart';
import '../../charging/views/map_screen.dart';


class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _formKey = GlobalKey<FormState>();
  final _userController = TextEditingController();
  final _passController = TextEditingController();
  
  bool _isLoading = false;
  bool _obscurePass = true;
  final ApiClient _apiClient = ApiClient();

  /// Executa a lógica de autenticação via BFF -> Keycloak
  Future<void> _handleLogin() async {
    // 1. Valida os campos do formulário
    if (!_formKey.currentState!.validate()) return;

    setState(() => _isLoading = true);

    try {
      // 2. Chama o método de login centralizado no ApiClient
      final success = await _apiClient.login(
        _userController.text.trim(),
        _passController.text,
      );

      if (success) {
        if (!mounted) return;

        // 3. Redirecionamento Inteligente:
        // pushAndRemoveUntil limpa a pilha de telas para que o botão "Voltar"
        // do Android não retorne o usuário deslogado para a tela de login.
        Navigator.pushAndRemoveUntil(
          context,
          MaterialPageRoute(builder: (context) => const MapScreen()),
          (route) => false,
        );
      } else {
        _showErrorSnackBar("Usuário ou senha inválidos. Verifique seu cadastro.");
      }
    } catch (e) {
      _showErrorSnackBar("Falha na conexão. Verifique se o BFF e Keycloak estão ativos.");
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  void _showErrorSnackBar(String message) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(message),
        backgroundColor: Colors.redAccent,
        behavior: SnackBarBehavior.floating,
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    // Cores baseadas no tema sustentável/elétrico do PowerBNB
    const primaryColor = Color(0xFF2E7D32); 

    return Scaffold(
      backgroundColor: Colors.white,
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.symmetric(horizontal: 32, vertical: 60),
          child: Form(
            key: _formKey,
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                // Identidade Visual
                const Icon(Icons.bolt_rounded, size: 80, color: primaryColor),
                const SizedBox(height: 12),
                const Text(
                  "PowerBNB",
                  textAlign: TextAlign.center,
                  style: TextStyle(
                    fontSize: 32,
                    fontWeight: FontWeight.bold,
                    color: primaryColor,
                    letterSpacing: 1.2,
                  ),
                ),
                const SizedBox(height: 8),
                const Text(
                  "SaaS de Carregamento Elétrico",
                  textAlign: TextAlign.center,
                  style: TextStyle(color: Colors.grey, fontSize: 14),
                ),
                const SizedBox(height: 60),

                // Campo de Usuário
                TextFormField(
                  controller: _userController,
                  keyboardType: TextInputType.emailAddress,
                  decoration: InputDecoration(
                    labelText: "Utilizador",
                    hintText: "Ex: admin ou e-mail",
                    prefixIcon: const Icon(Icons.person_outline, color: primaryColor),
                    border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                    focusedBorder: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(12),
                      borderSide: const BorderSide(color: primaryColor, width: 2),
                    ),
                  ),
                  validator: (v) => v!.isEmpty ? "Introduza o seu utilizador" : null,
                ),
                const SizedBox(height: 20),

                // Campo de Senha
                TextFormField(
                  controller: _passController,
                  obscureText: _obscurePass,
                  decoration: InputDecoration(
                    labelText: "Palavra-passe",
                    prefixIcon: const Icon(Icons.lock_outline, color: primaryColor),
                    suffixIcon: IconButton(
                      icon: Icon(_obscurePass ? Icons.visibility : Icons.visibility_off),
                      onPressed: () => setState(() => _obscurePass = !_obscurePass),
                    ),
                    border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
                    focusedBorder: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(12),
                      borderSide: const BorderSide(color: primaryColor, width: 2),
                    ),
                  ),
                  validator: (v) => v!.isEmpty ? "Introduza a sua palavra-passe" : null,
                ),

                const SizedBox(height: 10),
                Align(
                  alignment: Alignment.centerRight,
                  child: TextButton(
                    onPressed: () {
                      // Integração futura com o endpoint de reset de senha do Keycloak
                    },
                    child: const Text("Esqueceu-se da senha?", style: TextStyle(color: primaryColor)),
                  ),
                ),

                const SizedBox(height: 30),

                // Botão de Ação Principal
                ElevatedButton(
                  onPressed: _isLoading ? null : _handleLogin,
                  style: ElevatedButton.styleFrom(
                    backgroundColor: primaryColor,
                    foregroundColor: Colors.white,
                    padding: const EdgeInsets.symmetric(vertical: 18),
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                    elevation: 2,
                  ),
                  child: _isLoading
                      ? const SizedBox(
                          height: 20,
                          width: 20,
                          child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2),
                        )
                      : const Text(
                          "ENTRAR",
                          style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                        ),
                ),

                const SizedBox(height: 40),
                
                // Rodapé / Registro
                Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    const Text("Não tem uma conta?"),
                    TextButton(
                      onPressed: () {}, // Navegação para tela de Cadastro
                      child: const Text(
                        "Registe-se",
                        style: TextStyle(fontWeight: FontWeight.bold, color: primaryColor),
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  @override
  void dispose() {
    _userController.dispose();
    _passController.dispose();
    super.dispose();
  }
}