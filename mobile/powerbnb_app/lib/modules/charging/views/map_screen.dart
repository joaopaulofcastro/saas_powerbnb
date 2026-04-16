import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:latlong2/latlong.dart';
import 'package:powerbnb_app/modules/charging/views/register_point_screen.dart';

class MapScreen extends StatefulWidget {
  const MapScreen({super.key});

  @override
  State<MapScreen> createState() => _MapScreenState();
}

class _MapScreenState extends State<MapScreen> {
  // Coordenada inicial (usaremos o ponto que você cadastrou no banco: -3.876, -38.389)
  final LatLng _initialCenter = const LatLng(-3.876, -38.389);

  // Lista mockada de pontos para simular o layout da imagem
  final List<Marker> _markers = [];

  @override
  void initState() {
    super.initState();
    _loadMockMarkers();
  }

  void _loadMockMarkers() {
    // Aqui no futuro chamaremos o seu BFF / Endpoint .NET
    _markers.add(_buildCustomPin(const LatLng(-3.876, -38.389), true));
    _markers.add(_buildCustomPin(const LatLng(-3.870, -38.380), false));
    _markers.add(_buildCustomPin(const LatLng(-3.880, -38.395), true));
  }

  // Constrói o "Pin" verde com o raiozinho no meio igual à imagem
  Marker _buildCustomPin(LatLng position, bool isFastCharge) {
    return Marker(
      point: position,
      width: 40,
      height: 40,
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
                isFastCharge ? Icons.bolt : Icons.ev_station,
                size: 14,
                color: isFastCharge
                    ? Colors.orange
                    : Theme.of(context).primaryColor,
              ),
            ),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      // O Stack é crucial aqui: O mapa fica no fundo, a UI flutua por cima
      body: Stack(
        children: [
          // 1. CAMADA BASE: O MAPA
          FlutterMap(
            options: MapOptions(
              initialCenter: _initialCenter,
              initialZoom: 14.0,
            ),
            children: [
              TileLayer(
                // OpenStreetMap gratuito para começarmos
                urlTemplate: 'https://tile.openstreetmap.org/{z}/{x}/{y}.png',
                userAgentPackageName: 'com.saas.powerbnb.app',
              ),
              MarkerLayer(markers: _markers),
            ],
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
                  // Barra de Busca Flutuante
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
                        contentPadding: const EdgeInsets.symmetric(
                          vertical: 15,
                        ),
                      ),
                    ),
                  ),
                  const SizedBox(height: 10),
                  // Chips de Filtro (Carga Rápida, Disponíveis, etc)
                  SingleChildScrollView(
                    scrollDirection: Axis.horizontal,
                    child: Row(
                      children: [
                        _buildFilterChip('Carga Rápida', Icons.bolt),
                        const SizedBox(width: 8),
                        _buildFilterChip(
                          'Disponíveis',
                          Icons.check_circle_outline,
                        ),
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

      // 3. BOTÃO CENTRAL FLUTUANTE (Iniciar Carga)
      floatingActionButton: FloatingActionButton(
        onPressed: () {
          // Navega para a tela de registro
          Navigator.push(
            context,
            MaterialPageRoute(
              builder: (context) => RegisterPointScreen(
                initialLocation: LatLng(
                  -3.876,
                  -38.389,
                ), // Pega a posição central do mapa
              ),
            ),
          );
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

      // 4. BOTTOM BAR COM CORTE (Notch)
      bottomNavigationBar: BottomAppBar(
        shape: const CircularNotchedRectangle(),
        notchMargin: 8.0,
        child: Row(
          mainAxisAlignment: MainAxisAlignment.spaceAround,
          children: [
            _buildBottomNavIcon(Icons.map, 'Mapa', true),
            _buildBottomNavIcon(Icons.bookmark_border, 'Salvos', false),
            const SizedBox(width: 48), // Espaço vazio para o botão central
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
