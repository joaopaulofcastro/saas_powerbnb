import 'package:dio/dio.dart';
import '../models/charging_point_request.dart';
import '../models/nearby_point_dto.dart';
import '../models/update_point_request.dart';
import '../../../core/network/api_client.dart';

class ChargingRepository {
  final ApiClient _client;

  ChargingRepository(this._client);

  Future<bool> registerPoint(ChargingPointRequest request) async {
    try {
      final response = await _client.dio.post(
        '/bff/charging/charging-points',
        data: request.toJson(),
      );
      return response.statusCode == 201;
    } on DioException {
      return false;
    }
  }

  /// Busca pontos de carregamento disponíveis próximos à localização informada.
  /// Retorna lista vazia em caso de erro de rede.
  Future<List<NearbyPointDto>> getNearbyPoints(
    double lat,
    double lon,
    double radiusKm,
  ) async {
    final response = await _client.dio.get(
      '/bff/charging/charging-points/nearby',
      queryParameters: {
        'lat': lat,
        'lon': lon,
        'radiusKm': radiusKm,
      },
    );
    final data = response.data as List<dynamic>;
    return data
        .map((e) => NearbyPointDto.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  /// Edita os dados de um ponto de carregamento.
  /// Retorna o statusCode HTTP (204 = sucesso, 400 = validação, 403 = sem permissão).
  Future<int> updatePoint(String id, UpdatePointRequest request) async {
    try {
      final response = await _client.dio.put(
        '/bff/charging/charging-points/$id',
        data: request.toJson(),
      );
      return response.statusCode ?? 0;
    } on DioException catch (e) {
      return e.response?.statusCode ?? 0;
    }
  }

  /// Desativa (exclusão lógica) um ponto de carregamento.
  /// Retorna o statusCode HTTP (204 = sucesso, 403 = sem permissão, 409 = sessão ativa).
  Future<int> deletePoint(String id) async {
    try {
      final response = await _client.dio.delete(
        '/bff/charging/charging-points/$id',
      );
      return response.statusCode ?? 0;
    } on DioException catch (e) {
      return e.response?.statusCode ?? 0;
    }
  }
}
