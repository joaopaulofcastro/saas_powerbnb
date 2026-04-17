import 'package:dio/dio.dart';
import 'package:opentelemetry/api.dart';

// Implementação concreta do TextMapSetter para os headers do Dio
class _DioHeadersSetter implements TextMapSetter<Map<String, dynamic>> {
  const _DioHeadersSetter();

  @override
  void set(Map<String, dynamic> carrier, String key, String value) {
    carrier[key] = value;
  }
}

class TracingInterceptor extends Interceptor {
  static const _setter = _DioHeadersSetter();

  @override
  void onRequest(RequestOptions options, RequestInterceptorHandler handler) {
    final tracer = globalTracerProvider.getTracer('http-client');
    
    // 1. Cria o Span para a requisição de saída
    final span = tracer.startSpan(
      'HTTP ${options.method} ${options.path}',
      kind: SpanKind.client,
    );

    // 2. Adiciona atributos úteis para depuração
    span.setAttribute(Attribute.fromString('http.method', options.method));
    span.setAttribute(Attribute.fromString('http.url', options.uri.toString()));

    // 3. INJETA O TRACEPARENT
    globalTextMapPropagator.inject(
      contextWithSpan(Context.current, span),
      options.headers,
      _setter,
    );

    // Salva o span para fechar depois
    options.extra['span'] = span;

    super.onRequest(options, handler);
  }

  @override
  void onResponse(Response response, ResponseInterceptorHandler handler) {
    final span = response.requestOptions.extra['span'] as Span?;
    
    if (span != null) {
      span.setAttribute(Attribute.fromInt('http.status_code', response.statusCode ?? 200));
      span.setStatus(StatusCode.ok);
      span.end(); // 4. Finaliza o Span no sucesso
    }
    super.onResponse(response, handler);
  }

  @override
  void onError(DioException err, ErrorInterceptorHandler handler) {
    final span = err.requestOptions.extra['span'] as Span?;
    
    if (span != null) {
      span.setAttribute(Attribute.fromInt('http.status_code', err.response?.statusCode ?? 500));
      span.recordException(err);
      span.setStatus(StatusCode.error, err.message ?? 'Erro desconhecido');
      span.end(); // 4. Finaliza o Span com erro
    }
    super.onError(err, handler);
  }
}