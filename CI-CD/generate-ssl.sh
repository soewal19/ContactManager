#!/bin/bash

# Скрипт для генерации самоподписанного SSL сертификата для тестирования
# В продакшене используйте настоящие сертификаты от доверенного CA

echo "Генерация SSL сертификата для Contact Manager..."

# Создание директории для SSL
mkdir -p ssl

# Генерация приватного ключа
openssl genrsa -out ssl/key.pem 2048

# Генерация сертификата подписи запроса (CSR)
openssl req -new -key ssl/key.pem -out ssl/cert.csr -subj "/C=US/ST=State/L=City/O=ContactManager/CN=localhost"

# Генерация самоподписанного сертификата (действителен 365 дней)
openssl x509 -req -days 365 -in ssl/cert.csr -signkey ssl/key.pem -out ssl/cert.pem

# Установка правильных прав
chmod 600 ssl/key.pem
chmod 644 ssl/cert.pem

# Удаление временного CSR файла
rm ssl/cert.csr

echo "SSL сертификат сгенерирован!"
echo "Файлы:"
echo "  - ssl/cert.pem (сертификат)"
echo "  - ssl/key.pem (приватный ключ)"
echo ""
echo "ВНИМАНИЕ: Это самоподписанный сертификат для тестирования!"
echo "В продакшене используйте настоящие сертификаты от доверенного CA."