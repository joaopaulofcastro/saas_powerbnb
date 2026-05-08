import 'package:flutter/material.dart';
import '../models/nearby_point_dto.dart';
import '../models/update_point_request.dart';
import '../repositories/charging_repository.dart';
import '../../../core/network/api_client.dart';

class EditPointScreen extends StatefulWidget {
  final NearbyPointDto point;

  const EditPointScreen({super.key, required this.point});

  @override
  State<EditPointScreen> createState() => _EditPointScreenState();
}

class _EditPointScreenState extends State<EditPointScreen> {
  static const _primaryColor = Color(0xFF2E7D32);

  final _formKey = GlobalKey<FormState>();
  late final TextEditingController _titleController;
  late final TextEditingController _powerController;
  late final TextEditingController _priceController;
  late int _selectedConnector;

  bool _isSaving = false;
  bool _isDeactivating = false;

  late final ChargingRepository _repository;

  // Opções de conector alinhadas com o enum ConnectorType do backend
  static const _connectorOptions = [
    _ConnectorOption(value: 1, label: 'Type 2 (Padrão Europeu)'),
    _ConnectorOption(value: 2, label: 'GB/T (Padrão Chinês)'),
    _ConnectorOption(value: 3, label: 'CCS2 (Carga Rápida DC)'),
    _ConnectorOption(value: 4, label: 'Tomada Comum (WallPlug)'),
  ];

  @override
  void initState() {
    super.initState();
    _repository = ChargingRepository(ApiClient());
    _titleController = TextEditingController(text: widget.point.title);
    _powerController =
        TextEditingController(text: widget.point.maxPowerKw.toString());
    _priceController =
        TextEditingController(text: widget.point.pricePerKwh.toString());
    _selectedConnector = widget.point.connector;
  }

  @override
  void dispose() {
    _titleController.dispose();
    _powerController.dispose();
    _priceController.dispose();
    super.dispose();
  }

  bool get _isLoading => _isSaving || _isDeactivating;

  // ─── Salvar edição ────────────────────────────────────────────────────────

  Future<void> _savePoint() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() => _isSaving = true);

    try {
      final request = UpdatePointRequest(
        title: _titleController.text.trim(),
        connector: _selectedConnector,
        maxPowerKw: double.parse(_powerController.text.trim()),
        pricePerKwh: double.parse(_priceController.text.trim()),
      );

      final statusCode =
          await _repository.updatePoint(widget.point.id, request);

      if (!mounted) return;

      switch (statusCode) {
        case 204:
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(
              content: Text('Ponto atualizado com sucesso!'),
              backgroundColor: Colors.green,
            ),
          );
          Navigator.pop(context, true); // true = recarregar mapa
          break;
        case 400:
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(
              content: Text('Dados inválidos. Verifique os campos e tente novamente.'),
              backgroundColor: Colors.orange,
            ),
          );
          break;
        case 403:
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(
              content: Text('Você não tem permissão para editar este ponto.'),
              backgroundColor: Colors.redAccent,
            ),
          );
          break;
        default:
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(
              content: Text('Erro ao atualizar o ponto. Tente novamente.'),
              backgroundColor: Colors.redAccent,
            ),
          );
      }
    } catch (_) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Erro de conexão com o servidor.'),
            backgroundColor: Colors.redAccent,
          ),
        );
      }
    } finally {
      if (mounted) setState(() => _isSaving = false);
    }
  }

  // ─── Desativar ponto ──────────────────────────────────────────────────────

  Future<void> _showDeactivateDialog() async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Desativar ponto'),
        content: Text(
          'Tem certeza que deseja desativar "${widget.point.title}"?\n\n'
          'O ponto deixará de aparecer nas buscas e não aceitará novas sessões de recarga.',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child: const Text('Cancelar'),
          ),
          ElevatedButton(
            style: ElevatedButton.styleFrom(backgroundColor: Colors.red),
            onPressed: () => Navigator.pop(ctx, true),
            child: const Text(
              'Desativar',
              style: TextStyle(color: Colors.white),
            ),
          ),
        ],
      ),
    );

    if (confirmed == true) {
      await _deactivatePoint();
    }
  }

  Future<void> _deactivatePoint() async {
    setState(() => _isDeactivating = true);

    try {
      final statusCode = await _repository.deletePoint(widget.point.id);

      if (!mounted) return;

      switch (statusCode) {
        case 204:
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(
              content: Text('Ponto desativado com sucesso.'),
              backgroundColor: Colors.green,
            ),
          );
          Navigator.pop(context, true); // true = recarregar mapa
          break;
        case 403:
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(
              content: Text('Você não tem permissão para desativar este ponto.'),
              backgroundColor: Colors.redAccent,
            ),
          );
          break;
        case 400:
          // Point.Occupied retorna 400 com código "Point.Occupied"
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(
              content: Text(
                'Não é possível desativar: há uma sessão de recarga ativa neste ponto.',
              ),
              backgroundColor: Colors.orange,
            ),
          );
          break;
        default:
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(
              content: Text('Erro ao desativar o ponto. Tente novamente.'),
              backgroundColor: Colors.redAccent,
            ),
          );
      }
    } catch (_) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Erro de conexão com o servidor.'),
            backgroundColor: Colors.redAccent,
          ),
        );
      }
    } finally {
      if (mounted) setState(() => _isDeactivating = false);
    }
  }

  // ─── Build ────────────────────────────────────────────────────────────────

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Editar Ponto de Recarga'),
        backgroundColor: _primaryColor,
        foregroundColor: Colors.white,
        actions: [
          IconButton(
            icon: _isDeactivating
                ? const SizedBox(
                    width: 20,
                    height: 20,
                    child: CircularProgressIndicator(
                      color: Colors.white,
                      strokeWidth: 2,
                    ),
                  )
                : const Icon(Icons.delete_outline),
            tooltip: 'Desativar ponto',
            onPressed: _isLoading ? null : _showDeactivateDialog,
          ),
        ],
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(20),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              // Informação de localização (somente leitura)
              Container(
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(
                  color: Colors.grey.shade100,
                  borderRadius: BorderRadius.circular(8),
                  border: Border.all(color: Colors.grey.shade300),
                ),
                child: Row(
                  children: [
                    const Icon(Icons.location_on, color: Colors.grey, size: 18),
                    const SizedBox(width: 8),
                    Text(
                      'Lat: ${widget.point.latitude.toStringAsFixed(6)}, '
                      'Lon: ${widget.point.longitude.toStringAsFixed(6)}',
                      style: const TextStyle(color: Colors.grey, fontSize: 12),
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 4),
              const Text(
                'A localização não pode ser alterada após o cadastro.',
                style: TextStyle(color: Colors.grey, fontSize: 11),
              ),
              const SizedBox(height: 20),

              // Título
              TextFormField(
                controller: _titleController,
                enabled: !_isLoading,
                decoration: _inputDecoration('Título', Icons.title),
                validator: (v) {
                  if (v == null || v.trim().isEmpty) {
                    return 'O título não pode ser vazio.';
                  }
                  if (v.trim().length < 3) {
                    return 'O título deve ter no mínimo 3 caracteres.';
                  }
                  return null;
                },
              ),
              const SizedBox(height: 16),

              // Tipo de conector
              DropdownButtonFormField<int>(
                initialValue: _selectedConnector,
                decoration: _inputDecoration('Tipo de Conector', Icons.electrical_services),
                items: _connectorOptions
                    .map((o) => DropdownMenuItem(
                          value: o.value,
                          child: Text(o.label),
                        ))
                    .toList(),
                onChanged: _isLoading
                    ? null
                    : (v) => setState(() => _selectedConnector = v!),
              ),
              const SizedBox(height: 16),

              // Potência máxima
              TextFormField(
                controller: _powerController,
                enabled: !_isLoading,
                keyboardType: const TextInputType.numberWithOptions(decimal: true),
                decoration: _inputDecoration('Potência Máxima (kW)', Icons.bolt),
                validator: (v) {
                  final parsed = double.tryParse(v ?? '');
                  if (parsed == null || parsed <= 0) {
                    return 'Informe uma potência maior que zero.';
                  }
                  return null;
                },
              ),
              const SizedBox(height: 16),

              // Preço por kWh
              TextFormField(
                controller: _priceController,
                enabled: !_isLoading,
                keyboardType: const TextInputType.numberWithOptions(decimal: true),
                decoration: _inputDecoration('Preço por kWh (R\$)', Icons.attach_money),
                validator: (v) {
                  final parsed = double.tryParse(v ?? '');
                  if (parsed == null || parsed <= 0) {
                    return 'Informe um preço maior que zero.';
                  }
                  return null;
                },
              ),
              const SizedBox(height: 32),

              // Botão Salvar
              ElevatedButton(
                onPressed: _isLoading ? null : _savePoint,
                style: ElevatedButton.styleFrom(
                  backgroundColor: _primaryColor,
                  foregroundColor: Colors.white,
                  padding: const EdgeInsets.symmetric(vertical: 16),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                ),
                child: _isSaving
                    ? const SizedBox(
                        height: 20,
                        width: 20,
                        child: CircularProgressIndicator(
                          color: Colors.white,
                          strokeWidth: 2,
                        ),
                      )
                    : const Text(
                        'SALVAR ALTERAÇÕES',
                        style: TextStyle(
                          fontSize: 16,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
              ),
              const SizedBox(height: 12),

              // Botão Desativar
              OutlinedButton.icon(
                onPressed: _isLoading ? null : _showDeactivateDialog,
                icon: const Icon(Icons.power_off, color: Colors.red),
                label: const Text(
                  'Desativar Ponto',
                  style: TextStyle(color: Colors.red),
                ),
                style: OutlinedButton.styleFrom(
                  padding: const EdgeInsets.symmetric(vertical: 14),
                  side: const BorderSide(color: Colors.red),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  InputDecoration _inputDecoration(String label, IconData icon) {
    return InputDecoration(
      labelText: label,
      prefixIcon: Icon(icon, color: _primaryColor),
      border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
      focusedBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: const BorderSide(color: _primaryColor, width: 2),
      ),
    );
  }
}

class _ConnectorOption {
  final int value;
  final String label;
  const _ConnectorOption({required this.value, required this.label});
}
