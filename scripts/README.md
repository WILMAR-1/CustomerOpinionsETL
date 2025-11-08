# Scripts de Utilidades

Scripts auxiliares para el proyecto Customer Opinions ETL.

## generate_test_data.py

Script para generar datos de prueba para el proceso ETL.

### Descripción

Genera un archivo CSV con 500,001 registros de opiniones de clientes con datos realistas para probar el rendimiento del Worker Service.

### Características

- Genera 500,001 registros (cumpliendo el requisito de 500K+)
- Datos realistas: productos, clientes, países, ciudades
- Distribución natural de ratings (más ratings positivos que negativos)
- Comentarios coherentes con los ratings
- Sentiment scores calculados automáticamente
- Fechas de encuesta distribuidas en los últimos 2 años

### Requisitos

Python 3.7 o superior (sin dependencias externas, solo librerías estándar)

### Uso

```bash
# Desde la raíz del proyecto
python scripts/generate_test_data.py
```

### Salida

El script genera el archivo:
```
data/surveys.csv
```

Con las siguientes columnas:
- ProductId
- ProductName
- Category
- CustomerId
- CustomerName
- Country
- City
- SurveyDate
- Rating
- Sentiment
- Comment

### Rendimiento Esperado

- Tiempo de generación: 5-15 segundos (dependiendo del hardware)
- Tamaño del archivo: ~60-80 MB
- Tasa de generación: ~40,000-100,000 registros/segundo

### Ejemplo de Salida

```
Generando 500,001 registros de prueba...
Archivo de salida: C:\...\CustomerOpinionsETL\data\surveys.csv
  50,000 registros generados (45,000 registros/seg)
  100,000 registros generados (48,000 registros/seg)
  ...
  500,000 registros generados (50,000 registros/seg)

✓ Generación completada!
  Total de registros: 500,001
  Tiempo total: 10.50 segundos
  Tasa promedio: 47,620 registros/segundo
  Tamaño del archivo: 72.45 MB

Archivo generado: C:\...\CustomerOpinionsETL\data\surveys.csv
```

## Notas

- El archivo generado (data/surveys.csv) está incluido en .gitignore
- Los datos son ficticios y generados aleatoriamente
- El script sobrescribe el archivo si ya existe
