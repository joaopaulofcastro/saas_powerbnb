class ChargingPointRequest {
  final String title;
  final double pricePerKwh;
  final double lat;
  final double lon;
  final double maxPowerKw;
  final int connectorType;
  final String hostId;

  ChargingPointRequest({
    required this.title,
    required this.pricePerKwh,
    required this.lat,
    required this.lon,
    required this.maxPowerKw,
    required this.connectorType,
    required this.hostId,
  });

  Map<String, dynamic> toJson() => {
    'title': title,
    'pricePerKwh': pricePerKwh,
    'lat': lat,
    'lon': lon,
    'maxPowerKw': maxPowerKw,
    'connectorType': connectorType,
    'hostId': hostId
  };
}