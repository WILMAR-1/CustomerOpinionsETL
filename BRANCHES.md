# Gestión de Ramas del Proyecto

Este documento describe la estrategia de ramas utilizada en el proyecto Customer Opinions ETL.

## Ramas Principales

### `master`
Rama principal de desarrollo. Contiene la versión más actualizada del proyecto.

### `feature/etl-extraction-v1`
Rama dedicada a la **Práctica 1: Proceso de Extracción ETL**.

**Contenido:**
- Worker Service con proceso ETL completo
- Extractores: CSV, Base de Datos, API REST
- DataLoader para carga en DW_Opiniones
- Clean Architecture (Domain, Application, Infrastructure, Worker)
- Logging con Serilog
- Procesamiento asíncrono y paralelo

**Tag asociado:** `v1.0-extraction`

**Uso:**
```bash
# Cambiar a la rama de extracción
git checkout feature/etl-extraction-v1

# Ver el código de la práctica 1
git log --oneline
```

## Estructura de Ramas Futuras

### `feature/etl-transformation-v2` (Futura)
Para la práctica de transformación de datos.

### `feature/etl-load-optimization-v3` (Futura)
Para optimizaciones de carga y rendimiento.

### `feature/api-rest-v4` (Futura)
Para agregar la Web API con endpoints de consulta e inserción.

### `feature/dashboard-v5` (Futura)
Para el módulo de visualización/dashboard.

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

# De vuelta a master
git checkout master
```

### Crear nueva rama para siguiente práctica
```bash
# Desde master
git checkout master

# Crear rama para transformación
git checkout -b feature/etl-transformation-v2
```

### Ver diferencias entre ramas
```bash
git diff master..feature/etl-extraction-v1
```

## Estrategia de Versionado

| Versión | Tag | Rama | Descripción |
|---------|-----|------|-------------|
| v1.0 | v1.0-extraction | feature/etl-extraction-v1 | Proceso de Extracción |
| v2.0 | v2.0-transformation | feature/etl-transformation-v2 | Proceso de Transformación |
| v3.0 | v3.0-load-complete | feature/etl-load-optimization-v3 | ETL Completo Optimizado |
| v4.0 | v4.0-api | feature/api-rest-v4 | Web API REST |
| v5.0 | v5.0-dashboard | feature/dashboard-v5 | Dashboard de Visualización |

## Integración

Cuando una práctica esté completa y aprobada:

1. Asegurar que todos los cambios están commiteados
2. Mergear la rama a master:
   ```bash
   git checkout master
   git merge feature/etl-extraction-v1
   ```
3. Crear tag de release
4. Push al repositorio remoto (cuando esté configurado)

## Notas

- Cada práctica debe tener su propia rama
- Mantener master siempre en estado estable
- Usar tags para marcar entregas
- Documentar cambios significativos en commits descriptivos
