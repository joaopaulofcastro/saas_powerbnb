class NearbyPointDto {
  final String id;
  final String title;
  final double latitude;
  final double longitude;
  final int connector;
  final double maxPowerKw;
  final double pricePerKwh;
  final double distanceKm;

  NearbyPointDto({
    required this.id,
    required this.title,
    required this.latitude,
    required this.longitude,
    required this.connector,
    required this.maxPowerKw,
    required this.pricePerKwh,
    required this.distanceKm,
  });

  factory NearbyPointDto.fromJson(Map<String, dynamic> json) => NearbyPointDto(
        id: json['id'] as String,
        title: json['title'] as String,
        latitude: (json['latitude'] as num).toDouble(),
        longitude: (json['longitude'] as num).toDouble(),
        connector: json['connector'] as int,
        maxPowerKw: (json['maxPowerKw'] as num).toDouble(),
        pricePerKwh: (json['pricePerKwh'] as num).toDouble(),
        distanceKm: (json['distanceKm'] as num).toDouble(),
      );

  bool get isFastCharge => maxPowerKw > 22.0;

  String get connectorLabel {
    switch (connector) {
      case 1:
        return 'Type 2';
      case 2:
        return 'GB/T';
      case 3:
        return 'CCS2';
      case 4:
        return 'Tomada Comum';
      default:
        return 'Desconhecido';
    }
  }
}
