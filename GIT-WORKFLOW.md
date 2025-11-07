# Flujo de Trabajo con Git - Customer Opinions ETL

## Regla Principal

**TODOS los cambios deben ir primero a la rama feature, NUNCA directamente a master.**

## Flujo de Desarrollo

### 1. Trabajar en Feature Branch

```bash
# Asegurarse de estar en la rama feature
git checkout feature/etl-extraction-v1

# Verificar rama actual
git branch --show-current

# Hacer cambios en el código...

# Agregar cambios
git add .

# Hacer commit
git commit -m "Descripción del cambio"

# Ver estado
git status
```

### 2. Ver Progreso

```bash
# Ver commits que están en feature pero no en master
git log master..feature/etl-extraction-v1 --oneline

# Ver todos los commits
git log --oneline --graph --all -10
```

### 3. Cuando la Práctica Esté Completa

```bash
# 1. Asegurarse que todo está commiteado en feature
git status

# 2. Cambiar a master
git checkout master

# 3. Mergear feature a master
git merge feature/etl-extraction-v1

# 4. Crear/actualizar tag de versión
git tag -a v1.0-extraction -f -m "Version 1.0 - ETL Extraction Process (Practica 1)"

# 5. Verificar
git log --oneline -5
```

## Comandos Útiles

### Ver estado actual
```bash
# Mostrar ramas y commits
git branch -v

# Ver rama actual
git branch --show-current

# Ver diferencias entre ramas
git diff master..feature/etl-extraction-v1
```

### Corregir errores comunes

Si accidentalmente hiciste commit en master:

```bash
# 1. Ver el commit que necesitas mover
git log --oneline -3

# 2. Copiar el hash del commit (ej: abc1234)

# 3. Ir a feature
git checkout feature/etl-extraction-v1

# 4. Traer el commit de master
git cherry-pick abc1234

# 5. Volver a master y resetear
git checkout master
git reset --hard HEAD~1
```

## Estructura de Ramas

```
master (producción/entregable)
  │
  └── feature/etl-extraction-v1 (desarrollo)
       ├── commit 1
       ├── commit 2
       ├── commit 3
       └── [cuando esté listo] → merge a master
```

## Mejores Prácticas

1. **Siempre verificar** en qué rama estás antes de hacer commit
2. **Commits frecuentes** con mensajes descriptivos
3. **Probar antes de mergear** a master
4. **Master siempre estable** - solo código que funciona
5. **Feature para desarrollo** - donde se hacen todos los cambios

## Ejemplo de Sesión de Trabajo

```bash
# Inicio de sesión
cd CustomerOpinionsETL
git checkout feature/etl-extraction-v1

# Hacer cambios...
# Editar archivos

# Guardar cambios
git add .
git commit -m "Agregar validación de datos en CsvExtractor"

# Más cambios...
# Editar otros archivos

git add .
git commit -m "Optimizar consulta en DatabaseExtractor"

# Ver progreso
git log --oneline -5

# Cuando termines la práctica
git checkout master
git merge feature/etl-extraction-v1
git tag -a v1.0-extraction -f -m "Release v1.0"

# Fin
```

## Recordatorio

**NUNCA hacer `git checkout master` para hacer cambios.**
**SIEMPRE trabajar en `feature/etl-extraction-v1` primero.**
