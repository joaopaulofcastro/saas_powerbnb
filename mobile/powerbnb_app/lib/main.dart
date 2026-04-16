import 'package:flutter/material.dart';
import 'package:powerbnb_app/modules/charging/views/map_screen.dart';

void main() {
  // Aqui poderias inicializar serviços globais (como o Firebase ou Injeção de Dependência)
  runApp(const PowerBnbApp());
}

class PowerBnbApp extends StatelessWidget {
  const PowerBnbApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'PowerBNB',
      debugShowCheckedModeBanner: false,
      
      // Definição do Tema (Branding que definimos)
      theme: ThemeData(
        useMaterial3: true,
        colorScheme: ColorScheme.fromSeed(
          seedColor: const Color.fromARGB(255, 46, 125, 50), // Verde Sustentável
          brightness: Brightness.light,
        ),
        // Personalização global de botões e inputs
        inputDecorationTheme: const InputDecorationTheme(
          border: OutlineInputBorder(),
          filled: true,
        ),
        elevatedButtonTheme: ElevatedButtonThemeData(
          style: ElevatedButton.styleFrom(
            minimumSize: const Size.fromHeight(50),
            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
          ),
        ),
      ),

      // Define a tela de registo como a inicial para os nossos testes
      home: const MapScreen(),
    );
  }
}