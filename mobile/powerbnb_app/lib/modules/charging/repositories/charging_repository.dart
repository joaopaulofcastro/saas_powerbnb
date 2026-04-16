import '../models/charging_point_request.dart';
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
    } catch (e) {
      print(e.toString());
      return false;
    }
  }
}