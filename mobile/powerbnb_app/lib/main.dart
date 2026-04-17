import 'package:flutter/material.dart';
import 'package:powerbnb_app/infra/telemetry_setup.dart';
import 'core/auth/auth_service.dart';
import 'modules/auth/views/login_screen.dart';
import 'modules/charging/views/map_screen.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  configureOpenTelemetry();
  runApp(const PowerBNBApp());
}

final GlobalKey<NavigatorState> navigatorKey = GlobalKey<NavigatorState>();

class PowerBNBApp extends StatelessWidget {
  const PowerBNBApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      navigatorKey: navigatorKey,
      debugShowCheckedModeBanner: false,
      title: 'PowerBNB',
      theme: ThemeData(
        primarySwatch: Colors.green,
        useMaterial3: true,
      ),
      // O FutureBuilder atua como o Guardião da Rota Inicial
      home: FutureBuilder<bool>(
        future: AuthService().isAuthenticated(),
        builder: (context, snapshot) {
          // 1. Enquanto a verificação no Secure Storage está em curso
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Scaffold(
              body: Center(
                child: CircularProgressIndicator(color: Color(0xFF2E7D32)),
              ),
            );
          }

          // 2. Se a autenticação for confirmada (Token existe e é válido)
          if (snapshot.hasData && snapshot.data == true) {
            return const MapScreen();
          }

          // 3. Caso contrário (ou se houver erro), redireciona para o Login
          return const LoginScreen();
        },
      ),
    );
  }
}