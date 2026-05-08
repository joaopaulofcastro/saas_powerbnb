class UpdatePointRequest {
  final String title;
  final int connector;
  final double maxPowerKw;
  final double pricePerKwh;

  UpdatePointRequest({
    required this.title,
    required this.connector,
    required this.maxPowerKw,
    required this.pricePerKwh,
  });

  Map<String, dynamic> toJson() => {
        'title': title,
        'connector': connector,
        'maxPowerKw': maxPowerKw,
        'pricePerKwh': pricePerKwh,
      };
}
