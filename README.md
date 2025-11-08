# Customer Opinions ETL System

Sistema **ETL (Extract, Transform, Load)** desarrollado en **.NET 9** para la extracción, transformación y carga de opiniones de clientes desde múltiples fuentes de datos hacia un **Data Warehouse** analítico.

## Información del Proyecto

**Institución:** Universidad
**Asignatura:** Electiva 1 - Diseño e Implementación de Data Warehouse
**Estudiante:** Wilmar Gómez de la Cruz (2024-0103)
**Profesor:** Francis Ramírez
**Periodo:** Noviembre 2025

## Descripción

Este proyecto implementa un proceso ETL completo que extrae datos de opiniones de clientes desde tres fuentes distintas:

* **Archivos CSV** (encuestas internas)
* **Base de datos relacional** (reseñas web)
* **API REST** (comentarios de redes sociales)

Los datos extraídos son transformados, validados y cargados en un **Data Warehouse** basado en un **modelo dimensional (Star Schema)** para su posterior análisis.

## Objetivos

1. Diseñar una arquitectura orientada a servicios que soporte un proceso ETL escalable.
2. Desarrollar componentes de extracción según el tipo de fuente de datos.
3. Aplicar principios de calidad arquitectónica: rendimiento, seguridad, mantenibilidad y escalabilidad.
4. Documentar el flujo de extracción y justificar las decisiones arquitectónicas.

## Arquitectura

El proyecto implementa el patrón **Clean Architecture** con separación en cuatro capas principales:

```
CustomerOpinionsETL/
│
├── Application/         # Lógica de negocio, casos de uso y servicios de aplicación
├── Domain/              # Entidades, objetos de valor y lógica de dominio
├── Infrastructure/      # Conexiones a fuentes de datos, repositorios y adaptadores
└── Presentation/        # Consola o interfaz para ejecutar los procesos ETL
```

## Tecnologías Utilizadas

* **.NET 9**
* **C# 12**
* **Entity Framework Core**
* **Dapper**
* **PostgreSQL**
* **CSVHelper**
* **Newtonsoft.Json**

## Flujo General del Proceso ETL

1. **Extracción:** Obtiene datos desde las tres fuentes definidas.
2. **Transformación:** Normaliza, valida y unifica los registros según el modelo de negocio.
3. **Carga:** Inserta los datos procesados en el Data Warehouse para análisis OLAP.

## Estructura del Data Warehouse

El modelo de datos implementa un **Esquema Estrella (Star Schema)** con las siguientes tablas:

* **Tabla de Hechos:** `FactCustomerOpinion`
* **Tablas Dimensión:**

  * `DimCustomer`
  * `DimProduct`
  * `DimTime`
  * `DimSource`

## Ejecución

Para ejecutar el proceso ETL:

```bash
dotnet run --project Presentation
```

## Licencia

Proyecto académico desarrollado exclusivamente con fines educativos.


