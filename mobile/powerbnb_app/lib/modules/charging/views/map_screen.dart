import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:geolocator/geolocator.dart';
import 'package:latlong2/latlong.dart';
import 'package:powerbnb_app/modules/charging/models/nearby_point_dto.dart';
import 'package:powerbnb_app/modules/charging/repositories/charging_repository.dart';
import 'package:powerbnb_app/modules/charging/views/edit_point_screen.dart';
import 'package:powerbnb_app/modules/charging/views/register_point_screen.dart';
import '../../../core/network/api_client.dart';

class MapScreen extends StatefulWidget {
  const MapScreen({super.key});

  @override
  State<MapScreen> createState() => _MapScreenState();
}

class _MapScreenState extends State<MapScreen> {
  // Coordenada padrão (Fortaleza) usada quando a permissão é negada
  static const LatLng _defaultCenter = LatLng(-3.7172, -38.5433);
  static const double _defaultRadiusKm = 10.0;

  LatLng _currentCenter = _defaultCenter;
  final List<Marker> _markers = [];
  String? _currentUserId;
  bool _isLoading = false;

  late final ChargingRepository _repository;

  @override
  void initState() {
    super.initState();
    _repository = ChargingRepository(ApiClient());
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _init();
    });
  }

  Future<void> _init() async {
    await _loadCurrentUserId();
    await _loadNearbyPoints();
  }

  /// Lê o userId do JWT armazenado para identificar pontos do host.
  Future<void> _loadCurrentUserId() async {
    const storage = FlutterSecureStorage();
    final token = await storage.read(key: 'jwt_token');
    if (token != null) {
      // Extrai o sub do JWT (payload base64) de forma simples
      try {
        final parts = token.split('.');
        if (parts.length == 3) {
          final payload = parts[1];
          final normalized = base64Url.normalize(payload);
          final decoded = utf8.decode(base64Url.decode(normalized));
          final json = jsonDecode(decoded) as Map<String, dynamic>;
          setState(() => _currentUserId = json['sub'] as String?);
        }
      } catch (_) {
        // Ignora erros de parsing — botão Editar simplesmente não aparece
      }
    }
  }

  Future<void> _loadNearbyPoints() async {
    setState(() => _isLoading = true);

    try {
      final position = await _getPosition();
      if (position != null) {
        setState(() {
          _currentCenter = LatLng(position.latitude, position.longitude);
        });
      }

      final points = await _repository.getNearbyPoints(
        _currentCenter.latitude,
        _currentCenter.longitude,
        _defaultRadiusKm,
      );

      if (!mounted) return;

      setState(() {
        _markers.clear();
        for (final point in points) {
          _markers.add(_buildMarker(point));
        }
      });
    } catch (_) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Não foi possível carregar os pontos próximos.'),
            backgroundColor: Colors.redAccent,
          ),
        );
      }
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  /// Solicita permissão e retorna a posição atual, ou null se negada.
  Future<Position?> _getPosition() async {
    LocationPermission permission = await Geolocator.checkPermission();

    if (permission == LocationPermission.denied) {
      permission = await Geolocator.requestPermission();
    }

    if (permission == LocationPermission.denied ||
        permission == LocationPermission.deniedForever) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text(
              'Permissão de localização negada. Usando localização padrão.',
            ),
          ),
        );
      }
      return null;
    }

    return await Geolocator.getCurrentPosition(
      locationSettings: const LocationSettings(
        accuracy: LocationAccuracy.high,
      ),
    );
  }

  Marker _buildMarker(NearbyPointDto point) {
    return Marker(
      point: LatLng(point.latitude, point.longitude),
      width: 40,
      height: 40,
      child: GestureDetector(
        onTap: () => _showPointDetails(point),
        child: Stack(
          alignment: Alignment.center,
          children: [
            Icon(
              Icons.location_on,
              color: Theme.of(context).primaryColor,
              size: 40,
            ),
            Positioned(
              top: 6,
              child: Container(
                padding: const EdgeInsets.all(2),
                decoration: const BoxDecoration(
                  color: Colors.white,
                  shape: BoxShape.circle,
                ),
                child: Icon(
                  point.isFastCharge ? Icons.bolt : Icons.ev_station,
                  size: 14,
                  color: point.isFastCharge
                      ? Colors.orange
                      : Theme.of(context).primaryColor,
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  void _showPointDetails(NearbyPointDto point) {
    final isOwner = _currentUserId != null && _currentUserId == point.id;

    showModalBottomSheet(
      context: context,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      builder: (ctx) => Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Expanded(
                  child: Text(
                    point.title,
                    style: const TextStyle(
                      fontSize: 18,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ),
                if (isOwner)
                  TextButton.icon(
                    onPressed: () {
                      Navigator.pop(ctx);
                      Navigator.push(
                        context,
                        MaterialPageRoute(
                          builder: (_) => EditPointScreen(point: point),
                        ),
                      ).then((_) => _loadNearbyPoints());
                    },
                    icon: const Icon(Icons.edit, size: 16),
                    label: const Text('Editar'),
                  ),
              ],
            ),
            const Divider(),
            const SizedBox(height: 8),
            _detailRow(Icons.electrical_services, 'Conector', point.connectorLabel),
            _detailRow(Icons.bolt, 'Potência', '${point.maxPowerKw.toStringAsFixed(1)} kW'),
            _detailRow(Icons.attach_money, 'Preço', 'R\$ ${point.pricePerKwh.toStringAsFixed(2)}/kWh'),
            _detailRow(Icons.near_me, 'Distância', '${point.distanceKm.toStringAsFixed(2)} km'),
            const SizedBox(height: 12),
          ],
        ),
      ),
    );
  }

  Widget _detailRow(IconData icon, String label, String value) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4),
      child: Row(
        children: [
          Icon(icon, size: 18, color: const Color(0xFF2E7D32)),
          const SizedBox(width: 8),
          Text('$label: ', style: const TextStyle(fontWeight: FontWeight.w600)),
          Text(value),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Stack(
        children: [
          // 1. CAMADA BASE: O MAPA
          FlutterMap(
            options: MapOptions(
              initialCenter: _currentCenter,
              initialZoom: 14.0,
            ),
            children: [
              TileLayer(
                urlTemplate: 'https://tile.openstreetmap.org/{z}/{x}/{y}.png',
                userAgentPackageName: 'com.saas.powerbnb.app',
              ),
              MarkerLayer(markers: _markers),
            ],
          ),

          // Indicador de carregamento
          if (_isLoading)
            const Positioned(
              top: 80,
              left: 0,
              right: 0,
              child: Center(
                child: Card(
                  child: Padding(
                    padding: EdgeInsets.all(12),
                    child: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        SizedBox(
                          width: 16,
                          height: 16,
                          child: CircularProgressIndicator(strokeWidth: 2),
                        ),
                        SizedBox(width: 8),
                        Text('Buscando pontos próximos...'),
                      ],
                    ),
                  ),
                ),
              ),
            ),

          // 2. CAMADA SUPERIOR: BUSCA E FILTROS
          SafeArea(
            child: Padding(
              padding: const EdgeInsets.symmetric(
                horizontal: 16.0,
                vertical: 8.0,
              ),
              child: Column(
                children: [
                  Container(
                    decoration: BoxDecoration(
                      color: Colors.white,
                      borderRadius: BorderRadius.circular(30),
                      boxShadow: const [
                        BoxShadow(color: Colors.black12, blurRadius: 10),
                      ],
                    ),
                    child: TextField(
                      decoration: InputDecoration(
                        hintText: 'Pesquisar endereço...',
                        prefixIcon: const Icon(Icons.search),
                        suffixIcon: IconButton(
                          icon: const Icon(Icons.tune),
                          onPressed: () {},
                        ),
                        border: InputBorder.none,
                        contentPadding:
                            const EdgeInsets.symmetric(vertical: 15),
                      ),
                    ),
                  ),
                  const SizedBox(height: 10),
                  SingleChildScrollView(
                    scrollDirection: Axis.horizontal,
                    child: Row(
                      children: [
                        _buildFilterChip('Carga Rápida', Icons.bolt),
                        const SizedBox(width: 8),
                        _buildFilterChip(
                            'Disponíveis', Icons.check_circle_outline),
                        const SizedBox(width: 8),
                        _buildFilterChip('Gratuitos', Icons.money_off),
                      ],
                    ),
                  ),
                ],
              ),
            ),
          ),
        ],
      ),

      floatingActionButton: FloatingActionButton(
        onPressed: () {
          Navigator.push(
            context,
            MaterialPageRoute(
              builder: (context) => RegisterPointScreen(
                initialLocation: _currentCenter,
              ),
            ),
          ).then((_) => _loadNearbyPoints());
        },
        backgroundColor: const Color(0xFF2E7D32),
        shape: const CircleBorder(),
        child: const Icon(
          Icons.add_location_alt_rounded,
          color: Colors.white,
          size: 30,
        ),
      ),
      floatingActionButtonLocation: FloatingActionButtonLocation.centerDocked,

      bottomNavigationBar: BottomAppBar(
        shape: const CircularNotchedRectangle(),
        notchMargin: 8.0,
        child: Row(
          mainAxisAlignment: MainAxisAlignment.spaceAround,
          children: [
            _buildBottomNavIcon(Icons.map, 'Mapa', true),
            _buildBottomNavIcon(Icons.bookmark_border, 'Salvos', false),
            const SizedBox(width: 48),
            _buildBottomNavIcon(Icons.history, 'Histórico', false),
            _buildBottomNavIcon(Icons.person_outline, 'Perfil', false),
          ],
        ),
      ),
    );
  }

  Widget _buildFilterChip(String label, IconData icon) {
    return Chip(
      avatar: Icon(icon, size: 18, color: Theme.of(context).primaryColor),
      label: Text(label, style: const TextStyle(fontSize: 12)),
      backgroundColor: Colors.white,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
      elevation: 2,
      shadowColor: Colors.black12,
    );
  }

  Widget _buildBottomNavIcon(IconData icon, String label, bool isActive) {
    final color = isActive ? Theme.of(context).primaryColor : Colors.grey;
    return Column(
      mainAxisSize: MainAxisSize.min,
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        Icon(icon, color: color),
        Text(label, style: TextStyle(fontSize: 10, color: color)),
      ],
    );
  }
}
