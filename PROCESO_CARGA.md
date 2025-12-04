# Descripción del Proceso de Carga de Dimensiones y Hechos en CustomerOpinionsETL

Este documento describe el proceso de Extracción, Transformación y Carga (ETL) implementado en la aplicación CustomerOpinionsETL, con un enfoque particular en la carga de las tablas de dimensiones y hechos en el Data Warehouse.

## 1. Visión General del Proceso ETL

El sistema CustomerOpinionsETL está diseñado para recolectar opiniones de clientes de diversas fuentes, procesarlas y almacenarlas en un Data Warehouse para análisis. El flujo general es el siguiente:

1.  **Extracción (Extract):** Los datos de opiniones son obtenidos de múltiples fuentes configurables (APIs externas, archivos CSV, bases de datos, JSON).
2.  **Transformación (Transform):** Se aplican transformaciones mínimas, principalmente de validación y estandarización, para preparar los datos para su carga.
3.  **Carga (Load):** Los datos transformados se cargan en las tablas de dimensiones y hechos del Data Warehouse.

## 2. Componentes Clave del Pro Proceso

### 2.1. CustomerOpinionsETL.Worker (Punto de Entrada)

-   **Ruta:** `src/CustomerOpinionsETL.Worker/Worker.cs`
-   **Descripción:** Este proyecto actúa como un servicio en segundo plano (background service) que inicia y gestiona la ejecución periódica del proceso ETL. Su método `ExecuteAsync` es el punto de entrada principal que dispara la orquestación del ETL.

### 2.2. CustomerOpinionsETL.Application.Services.EtlService (Orquestador)

-   **Ruta:** `src/CustomerOpinionsETL.Application/Services/EtlService.cs`
-   **Descripción:** `EtlService` es el cerebro del proceso ETL. Es responsable de:
    *   Coordinar las implementaciones de `IExtractor` para obtener los datos de las diferentes fuentes.
    *   Realizar transformaciones básicas y validaciones.
    *   Delegar la carga final de los datos al componente `IDataLoader`.

### 2.3. CustomerOpinionsETL.Infrastructure.Persistence.DataLoader (Cargador de Datos)

-   **Ruta:** `src/CustomerOpinionsETL.Infrastructure/Persistence/DataLoader.cs`
-   **Descripción:** Este es el componente más crítico para la carga del Data Warehouse. Contiene la lógica robusta y optimizada para la inserción masiva de datos en las tablas de dimensiones y en la tabla de hechos (`FactOpinion`). El método clave es `LoadOpinionsAsync`, el cual realiza las siguientes operaciones en secuencia:

#### Proceso de Carga de Dimensiones:

El `DataLoader` procesa la información de las opiniones para identificar y cargar las dimensiones de manera eficiente. Para cada tipo de dimensión, se realiza un proceso similar que busca evitar duplicados y asegura la integridad referencial.

1.  **DimProduct (Dimensión de Producto)**
    *   **Método:** `ProcessProductDimensionBulkAsync` (o similar dentro de `LoadOpinionsAsync`)
    *   **Descripción:** Identifica los productos únicos de los datos de entrada. Si un producto ya existe en `DimProduct`, se utiliza su clave existente. Si es un nuevo producto, se inserta en la tabla y se obtiene su nueva clave. Esto garantiza que cada producto sea único en la dimensión.

2.  **DimCustomer (Dimensión de Cliente)**
    *   **Método:** `ProcessCustomerDimensionBulkAsync` (o similar dentro de `LoadOpinionsAsync`)
    *   **Descripción:** Extrae la información de los clientes de las opiniones. Similar a `DimProduct`, verifica si el cliente ya existe en `DimCustomer`. Si no, lo inserta y recupera su clave. Esto mantiene un registro único de cada cliente.

3.  **DimChannel (Dimensión de Canal)**
    *   **Método:** `ProcessChannelDimensionBulkAsync` (o similar dentro de `LoadOpinionsAsync`)
    *   **Descripción:** Identifica los canales únicos (e.g., "Web", "App", "Tienda"). Si el canal no existe en `DimChannel`, se inserta. Se utiliza la clave existente o la recién creada para futuras referencias.

4.  **DimDate (Dimensión de Fecha)**
    *   **Método:** `ProcessDateDimensionBulkAsync` (o similar dentro de `LoadOpinionsAsync`)
    *   **Descripción:** A partir de las fechas de las opiniones, se asegura de que cada fecha única (día, mes, año, etc.) esté presente en la tabla `DimDate`. Esta dimensión suele ser pre-cargada con un rango amplio de fechas, pero el `DataLoader` puede manejar la inserción de nuevas fechas si es necesario.

#### Proceso de Carga de la Tabla de Hechos:

Una vez que todas las dimensiones están pobladas y se han recuperado sus claves primarias correspondientes, el `DataLoader` procede a cargar la tabla de hechos:

*   **FactOpinion (Tabla de Hechos de Opiniones)**
    *   **Descripción:** Para cada opinión procesada, se crea un registro en `FactOpinion`. Este registro incluye las claves foráneas de `DimProduct`, `DimCustomer`, `DimChannel`, y `DimDate` que fueron obtenidas en el paso anterior, junto con las métricas o atributos propios de la opinión (e.g., puntuación, texto de la opinión). La carga se realiza de forma masiva para optimizar el rendimiento.

## 3. Conclusión

El módulo `DataLoader` es fundamental para el funcionamiento del Data Warehouse, asegurando una carga eficiente y coherente de las dimensiones y la tabla de hechos, lo que permite un análisis posterior fiable de las opiniones de los clientes. El proceso está diseñado para ser robusto y escalar con el volumen de datos.
