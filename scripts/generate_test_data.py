"""
Script para generar datos de prueba para el ETL de Customer Opinions
Genera 500,001 registros en formato CSV para probar el rendimiento del proceso ETL
"""

import csv
import random
from datetime import datetime, timedelta
from pathlib import Path

# Configuración
NUM_RECORDS = 500_001
OUTPUT_FILE = Path(__file__).parent.parent / "data" / "surveys.csv"

# Datos de muestra
PRODUCTS = [
    {"id": 1001, "name": "Laptop Pro X1", "category": "Electronics"},
    {"id": 1002, "name": "Smartphone Galaxy", "category": "Electronics"},
    {"id": 1003, "name": "Wireless Headphones", "category": "Electronics"},
    {"id": 1004, "name": "4K Monitor", "category": "Electronics"},
    {"id": 1005, "name": "Gaming Mouse", "category": "Electronics"},
    {"id": 2001, "name": "Office Chair Deluxe", "category": "Furniture"},
    {"id": 2002, "name": "Standing Desk", "category": "Furniture"},
    {"id": 2003, "name": "LED Desk Lamp", "category": "Furniture"},
    {"id": 3001, "name": "Running Shoes Pro", "category": "Sports"},
    {"id": 3002, "name": "Yoga Mat Premium", "category": "Sports"},
    {"id": 3003, "name": "Fitness Tracker", "category": "Sports"},
    {"id": 4001, "name": "Coffee Maker Deluxe", "category": "Appliances"},
    {"id": 4002, "name": "Blender Pro", "category": "Appliances"},
    {"id": 4003, "name": "Air Fryer XL", "category": "Appliances"},
    {"id": 5001, "name": "Winter Jacket", "category": "Clothing"},
    {"id": 5002, "name": "Casual Sneakers", "category": "Clothing"},
]

FIRST_NAMES = [
    "John", "Maria", "Carlos", "Ana", "Michael", "Laura", "David", "Sofia",
    "James", "Isabella", "Robert", "Camila", "William", "Valentina", "Richard",
    "Emma", "Jose", "Lucia", "Thomas", "Victoria", "Daniel", "Mariana",
    "Matthew", "Gabriela", "Christopher", "Daniela", "Antonio", "Sara"
]

LAST_NAMES = [
    "Smith", "Johnson", "Garcia", "Rodriguez", "Martinez", "Brown", "Davis",
    "Wilson", "Moore", "Taylor", "Anderson", "Thomas", "Jackson", "White",
    "Harris", "Martin", "Thompson", "Lopez", "Lee", "Gonzalez", "Hernandez",
    "Young", "King", "Wright", "Lopez", "Hill", "Scott", "Green", "Adams"
]

COUNTRIES_CITIES = {
    "United States": ["New York", "Los Angeles", "Chicago", "Houston", "Miami"],
    "Canada": ["Toronto", "Montreal", "Vancouver", "Calgary", "Ottawa"],
    "Mexico": ["Mexico City", "Guadalajara", "Monterrey", "Cancun", "Puebla"],
    "Spain": ["Madrid", "Barcelona", "Valencia", "Seville", "Bilbao"],
    "Argentina": ["Buenos Aires", "Cordoba", "Rosario", "Mendoza", "La Plata"],
    "Colombia": ["Bogota", "Medellin", "Cali", "Barranquilla", "Cartagena"],
    "Brazil": ["Sao Paulo", "Rio de Janeiro", "Brasilia", "Salvador", "Fortaleza"],
    "United Kingdom": ["London", "Manchester", "Birmingham", "Liverpool", "Edinburgh"],
}

POSITIVE_COMMENTS = [
    "Excellent product, exceeded my expectations!",
    "Great quality, very satisfied with my purchase.",
    "Amazing! Worth every penny.",
    "Best purchase I've made this year.",
    "Outstanding quality and fast delivery.",
    "Highly recommend this product to everyone.",
    "Perfect! Exactly what I was looking for.",
    "Impressive performance and design.",
    "Very happy with this purchase.",
    "Fantastic product, will buy again!"
]

NEUTRAL_COMMENTS = [
    "Product is okay, nothing special.",
    "Average quality for the price.",
    "It works as expected.",
    "Decent product, meets basic needs.",
    "Not bad, but could be better.",
    "Standard quality, nothing outstanding.",
    "It's fine for everyday use.",
    "Acceptable, meets minimum requirements.",
]

NEGATIVE_COMMENTS = [
    "Disappointed with the quality.",
    "Not what I expected, poor quality.",
    "Broke after a few days of use.",
    "Would not recommend this product.",
    "Waste of money, very disappointed.",
    "Poor customer service and defective product.",
    "Does not work as advertised.",
    "Terrible quality, returning it.",
]

def generate_customer_name():
    """Genera un nombre aleatorio de cliente"""
    first = random.choice(FIRST_NAMES)
    last = random.choice(LAST_NAMES)
    return f"{first} {last}"

def generate_comment(rating):
    """Genera un comentario basado en el rating"""
    if rating >= 4:
        return random.choice(POSITIVE_COMMENTS)
    elif rating == 3:
        return random.choice(NEUTRAL_COMMENTS)
    else:
        return random.choice(NEGATIVE_COMMENTS)

def calculate_sentiment(rating):
    """Calcula el sentiment score basado en el rating (escala -100 a 100)"""
    # Rating 5 -> 80-100
    # Rating 4 -> 40-79
    # Rating 3 -> -20-39
    # Rating 2 -> -60 a -21
    # Rating 1 -> -100 a -61
    ranges = {
        5: (80, 100),
        4: (40, 79),
        3: (-20, 39),
        2: (-60, -21),
        1: (-100, -61)
    }
    min_val, max_val = ranges[rating]
    return random.randint(min_val, max_val)

def generate_survey_date():
    """Genera una fecha aleatoria en los últimos 2 años"""
    end_date = datetime.now()
    start_date = end_date - timedelta(days=730)  # 2 años

    time_between = end_date - start_date
    days_between = time_between.days
    random_days = random.randrange(days_between)

    random_date = start_date + timedelta(days=random_days)
    return random_date.strftime("%Y-%m-%d")

def generate_record(customer_id):
    """Genera un registro de opinión"""
    product = random.choice(PRODUCTS)
    country = random.choice(list(COUNTRIES_CITIES.keys()))
    city = random.choice(COUNTRIES_CITIES[country])

    # Distribución de ratings: más ratings altos (4-5) que bajos
    rating = random.choices([1, 2, 3, 4, 5], weights=[5, 10, 20, 35, 30])[0]

    sentiment = calculate_sentiment(rating)
    comment = generate_comment(rating)

    return {
        "ProductId": product["id"],
        "ProductName": product["name"],
        "Category": product["category"],
        "CustomerId": customer_id,
        "CustomerName": generate_customer_name(),
        "Country": country,
        "City": city,
        "SurveyDate": generate_survey_date(),
        "Rating": rating,
        "Sentiment": sentiment,
        "Comment": comment
    }

def main():
    """Función principal"""
    print(f"Generando {NUM_RECORDS:,} registros de prueba...")
    print(f"Archivo de salida: {OUTPUT_FILE}")

    # Crear directorio data si no existe
    OUTPUT_FILE.parent.mkdir(parents=True, exist_ok=True)

    start_time = datetime.now()

    # Escribir CSV
    with open(OUTPUT_FILE, 'w', newline='', encoding='utf-8') as csvfile:
        fieldnames = [
            "ProductId", "ProductName", "Category",
            "CustomerId", "CustomerName", "Country", "City",
            "SurveyDate", "Rating", "Sentiment", "Comment"
        ]

        writer = csv.DictWriter(csvfile, fieldnames=fieldnames)
        writer.writeheader()

        # Generar registros
        for i in range(1, NUM_RECORDS + 1):
            record = generate_record(customer_id=i)
            writer.writerow(record)

            # Mostrar progreso cada 50,000 registros
            if i % 50_000 == 0:
                elapsed = (datetime.now() - start_time).total_seconds()
                rate = i / elapsed if elapsed > 0 else 0
                print(f"  {i:,} registros generados ({rate:,.0f} registros/seg)")

    # Estadísticas finales
    end_time = datetime.now()
    elapsed = (end_time - start_time).total_seconds()

    print(f"\n[OK] Generacion completada!")
    print(f"  Total de registros: {NUM_RECORDS:,}")
    print(f"  Tiempo total: {elapsed:.2f} segundos")
    print(f"  Tasa promedio: {NUM_RECORDS/elapsed:,.0f} registros/segundo")
    print(f"  Tamano del archivo: {OUTPUT_FILE.stat().st_size / (1024*1024):.2f} MB")
    print(f"\nArchivo generado: {OUTPUT_FILE}")

if __name__ == "__main__":
    main()
