# Gestión de Ramas del Proyecto

Este documento describe la estrategia de ramas utilizada en el proyecto Customer Opinions ETL.

## Ramas Principales

### `master`
Rama principal de desarrollo. Contiene la versión más actualizada del proyecto con todas las funcionalidades completadas y mergeadas.

### `feature/etl-extraction-v1` ✅ COMPLETADA
Rama dedicada a la **Práctica 1: Proceso de Extracción ETL**.

**Estado:** Mergeada a master

**Contenido:**
- Worker Service con proceso ETL completo
- Extractores: CSV, Base de Datos, API REST
- DataLoader con operaciones BULK puras (sin bucles)
- Clean Architecture (Domain, Application, Infrastructure, Worker)
- Separación de DTOs en archivos individuales (Models/Dto/)
- Logging con Serilog
- Procesamiento asíncrono y paralelo
- Procesamiento de 500K+ registros en ~4 segundos

**Tag asociado:** `v1.0-extraction`

**Uso:**
```bash
# Cambiar a la rama de extracción
git checkout feature/etl-extraction-v1

# Ver el código de la práctica 1
git log --oneline
```

### `feature/api-v2` 🚀 EN DESARROLLO
Rama activa para la **Práctica 2: API REST para consulta del Data Warehouse**.

**Estado:** En desarrollo

**Contenido planificado:**
- Proyecto ASP.NET Core Web API
- Endpoints para consultas analíticas del Data Warehouse
- Repositorio de lectura optimizado
- DTOs de respuesta para API
- Documentación con Swagger/OpenAPI
- Filtros, paginación y ordenamiento
- Métricas, agregaciones y tendencias
- CORS y seguridad básica

**Uso:**
```bash
# Cambiar a la rama de API
git checkout feature/api-v2

# Ver progreso
git log --oneline
```

## Estructura de Ramas Futuras

### `feature/advanced-analytics-v3` (Futura)
Para análisis avanzados, tendencias temporales y predicciones con ML.

### `feature/dashboard-v4` (Futura)
Para el módulo de visualización/dashboard con gráficos interactivos.

### `feature/real-time-etl-v5` (Futura)
Para procesamiento en tiempo real con cambios incrementales (CDC).

## Comandos Útiles

### Ver todas las ramas
```bash
git branch -a
```

### Ver tags
```bash
git tag -l
```

### Cambiar de rama
```bash
# A la rama de extracción (Práctica 1)
git checkout feature/etl-extraction-v1

# A la rama de API (Práctica 2)
git checkout feature/api-v2

# De vuelta a master
git checkout master
```

### Crear nueva rama para siguiente práctica
```bash
# Desde master
git checkout master

# Crear rama para nueva funcionalidad
git checkout -b feature/nueva-funcionalidad-v3
```

### Ver diferencias entre ramas
```bash
git diff master..feature/api-v2
```

## Estrategia de Versionado

| Versión | Tag | Rama | Estado | Descripción |
|---------|-----|------|--------|-------------|
| v1.0 | v1.0-extraction | feature/etl-extraction-v1 | ✅ Completada | Proceso ETL Completo con Extractores |
| v2.0 | v2.0-api | feature/api-v2 | 🚀 En desarrollo | Web API REST para consultas |
| v3.0 | v3.0-analytics | feature/advanced-analytics-v3 | 📅 Futura | Análisis Avanzados |
| v4.0 | v4.0-dashboard | feature/dashboard-v4 | 📅 Futura | Dashboard de Visualización |
| v5.0 | v5.0-realtime | feature/real-time-etl-v5 | 📅 Futura | ETL en Tiempo Real |

## Integración

Cuando una práctica está completa y aprobada:

1. Asegurar que todos los cambios estén commiteados en la rama feature
2. Cambiar a master: `git checkout master`
3. Mergear la rama feature a master:
   ```bash
   git merge feature/nombre-rama --no-ff -m "Mensaje descriptivo"
   ```
4. Resolver conflictos si existen
5. Crear tag de release: `git tag -a v1.0-extraction -m "Release v1.0"`
6. Push al repositorio remoto: `git push origin master --tags`

## Notas

- Cada práctica/funcionalidad debe tener su propia rama feature
- Mantener master siempre en estado estable
- Usar tags para marcar entregas y releases
- Documentar cambios significativos en commits descriptivos
- Separar DTOs, modelos y lógica en archivos individuales
- Seguir convenciones de nomenclatura de commits:
  - `Feat:` para nuevas funcionalidades
  - `Fix:` para corrección de bugs
  - `Refactor:` para refactorización de código
  - `Docs:` para documentación
  - `Perf:` para mejoras de rendimiento
  - `Sync:` para sincronización entre ramas
