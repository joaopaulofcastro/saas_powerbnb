import 'package:opentelemetry/api.dart';
import 'package:opentelemetry/sdk.dart';

void configureOpenTelemetry() {
  // Aponta para o seu BFF local (ou ambiente de dev)
  // O YARP no BFF precisará ter uma rota configurada para repassar o /v1/traces para o Collector
  final exporter = CollectorExporter(
    Uri.parse('http://172.168.0.71:5276/v1/traces'), // 10.0.2.2 é o localhost do emulador Android
  );

  final processor = BatchSpanProcessor(exporter);

  final provider = TracerProviderBase(
    processors: [processor],
    resource: Resource([
      Attribute.fromString('service.name', 'powerbnb-app'), // Identifica o app mobile no Jaeger
      Attribute.fromString('service.version', '1.0.0'),
      Attribute.fromString('os.name', 'Android/iOS'), 
    ]),
  );

  registerGlobalTracerProvider(provider);
}