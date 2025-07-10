# Product Service - Microservicio de Productos

Este servicio permite a usuarios autenticados crear y consultar productos, y genera descripciones automáticamente con ayuda de AI.

## 🧩 Funcionalidades

- CRUD de productos (creación y edición activas)
- Asociación de productos al `userId` autenticado
- Generación automática de descripción vía n8n + Gemini

## ⚙️ Tecnologías

- C# con .NET 8
- PostgreSQL (Supabase)
- Entity Framework Core
- JWT Authentication
- Webhook externo (n8n)

## 🔁 Flujo AI

1. Usuario crea producto
2. El backend dispara un webhook a n8n
3. n8n usa Gemini y responde con una descripción generada
4. El backend actualiza el producto con esta descripción

## 🧪 Cómo probar

1. Ejecutar: `dotnet run`
2. Visitar Swagger: `http://localhost:5181/swagger/index.html`
3. Requiere token JWT en los headers

## 🔐 Seguridad

- Token obligatorio en cada request (`Authorization: Bearer [token]`)
- El backend valida el token y asigna el producto al `userId`

## 🌐 Webhook usado

`https://n8n.srv885850.hstgr.cloud/webhook/3af0813f-6be9-4886-a6eb-294ea614eaf5`
