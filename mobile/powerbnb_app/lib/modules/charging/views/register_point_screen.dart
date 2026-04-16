import 'package:flutter/material.dart';
import 'package:latlong2/latlong.dart';
import '../models/charging_point_request.dart';
import '../repositories/charging_repository.dart';
import '../../../core/network/api_client.dart';

class RegisterPointScreen extends StatefulWidget {
  final LatLng? initialLocation;
  const RegisterPointScreen({super.key, this.initialLocation});

  @override
  State<RegisterPointScreen> createState() => _RegisterPointScreenState();
}

class _RegisterPointScreenState extends State<RegisterPointScreen> {
  bool _isLoading = false;
  final _formKey = GlobalKey<FormState>();
  final _titleController = TextEditingController();
  final _priceController = TextEditingController();
  final _powerController = TextEditingController();

  late TextEditingController _latController;
  late TextEditingController _lngController;

  @override
  void initState() {
    super.initState();
    // Inicializa os controllers com os valores passados ou vazios
    _latController = TextEditingController(
      text: widget.initialLocation?.latitude.toString() ?? ""
    );
    _lngController = TextEditingController(
      text: widget.initialLocation?.longitude.toString() ?? ""
    );
  }

  @override
  void dispose() {
    _latController.dispose();
    _lngController.dispose();
    super.dispose();
  }

  void _submit() async {
  if (_formKey.currentState!.validate()) {
    setState(() => _isLoading = true); // Adicione um bool _isLoading no State

    try {
      // Usamos a instância do ApiClient que já tem o Interceptor de Token
      final repo = ChargingRepository(ApiClient());

      final request = ChargingPointRequest(
        title: _titleController.text.trim(),
        pricePerKwh: double.parse(_priceController.text),
        lat: double.parse(_latController.text),
        lon: double.parse(_lngController.text),
        maxPowerKw: double.parse(_powerController.text),
        connectorType: 1, // Poderia vir de um Dropdown futuramente
      );

      final success = await repo.registerPoint(request);

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(success ? "Ponto cadastrado com sucesso!" : "Falha ao registrar ponto."),
            backgroundColor: success ? Colors.green : Colors.red,
          ),
        );
        
        // Se deu certo, volta para o Mapa para ver o novo ponto
        if (success) {
          Navigator.pop(context); 
        }
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text("Erro de conexão com o servidor.")),
        );
      }
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }
}

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text("Novo Ponto de Recarga"),
        backgroundColor: const Color(0xFF2E7D32),
        foregroundColor: Colors.white,
      ),
      body: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Form(
          key: _formKey,
          child: Column(
            children: [
              TextFormField(
                controller: _titleController,
                decoration: const InputDecoration(labelText: "Título"),
              ),
              TextFormField(
                controller: _priceController,
                decoration: const InputDecoration(labelText: "Preço por kWh"),
                keyboardType: TextInputType.number,
              ),
              TextFormField(
                controller: _powerController,
                decoration: const InputDecoration(
                  labelText: "Potência Máxima (kW)",
                ),
                keyboardType: TextInputType.number,
              ),
              const SizedBox(height: 20),
              ElevatedButton(
                onPressed: _submit,
                child: const Text("Registrar no SaaS"),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
