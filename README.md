
# 📘 CollabSkillHub

> Веб-застосунок для пошуку роботи в ІТ, що об'єднує замовників і виконавців з можливістю створення вакансій, подачі заявок та комунікації в реальному часі через чат.

---

## 👤 Автор

- **ПІБ**: Білецький Олег Ростиславович  
- **Група**: ФеП-41  
- **Керівник**: асистент Мисюк Роман Володимирович  
- **Дата виконання**: 01.06.2025

---

## 📌 Загальна інформація

- **Тип проєкту**: Вебсайт
- **Мова програмування**: C# (ASP.NET Core), JavaScript (React)
- **Фреймворки / Бібліотеки**: ASP.NET Core Web API, React, SignalR, Entity Framework Core, Bootstrap

---

## 🧠 Опис функціоналу

- 🔐 Реєстрація та авторизація з використанням JWT
- 👤 Профілі користувачів (Customer / Contractor)
- 📄 Створення та управління вакансіями
- ✉️ Подання заявок на вакансії
- 💬 Комунікація у реальному часі через чат (SignalR)
- 📈 Відгуки, статуси, управління проєктами
- 💵 Інтеграція з платіжними системами (основа PayPal)

---

## 🧱 Опис основних класів / файлів

| Клас / Файл             | Призначення                                      |
|------------------------|--------------------------------------------------|
| `ChatHub.cs`           | Реалізація хабу SignalR для обміну повідомленнями |
| `WorkController.cs`    | Обробка запитів, пов'язаних із вакансіями        |
| `RequestController.cs` | Обробка заявок від виконавців                    |
| `ChatService.cs`       | Сервіс для обробки чатів                        |
| `PaymentService.cs`    | Робота з платежами (статистика, транзакції)     |
| `AppDbContext.cs`      | Контекст бази даних (EF Core)                   |
| `Dashboard.jsx`        | Панель користувача у React                      |
| `ChatPage.jsx`         | Компонент чату на клієнті                       |

---

## ▶️ Як запустити проєкт "з нуля"

### 1. Встановлення інструментів

- [.NET 7 SDK](https://dotnet.microsoft.com/en-us/)
- Node.js (v18+)
- Microsoft SQL Server

### 2. Клонування репозиторію

```bash
git clone https://github.com/seemerslx/CollabSkillHub.git
cd CollabSkillHub
```

### 3. Налаштування Backend

- Створіть файл `appsettings.json` або `.env` для підключення до SQL Server
- Запустіть міграції:

```bash
dotnet ef database update
```

- Запуск:

```bash
dotnet run
```

### 4. Налаштування Frontend

```bash
cd client
npm install
npm start
```

---

## 🔌 API приклади

### 🔐 Авторизація

**POST /api/auth/login**

```json
{
  "email": "user@example.com",
  "password": "Password123"
}
```

### 📋 Вакансії

**GET /api/works** — список доступних вакансій  
**POST /api/works** — створення нової вакансії  
**POST /api/requests** — подати заявку  
**POST /chat/send** — надсилання повідомлення у чат

---

## 🖱️ Інструкція для користувача

- Користувач створює акаунт (вибір ролі: Замовник або Виконавець)
- Замовник:
  - Створює вакансії
  - Приймає або відхиляє заявки
  - Спілкується через чат після підтвердження
- Виконавець:
  - Переглядає вакансії
  - Подасть заявку на вподобану
  - Спілкується із замовником

---

## 📷 Приклади / скриншоти

(додайте зображення у папку `/screenshots/`)

---

## 🧪 Проблеми і рішення

| Проблема                        | Рішення                                      |
|--------------------------------|----------------------------------------------|
| Помилка 401                    | Перевірити наявність та валідність JWT токена |
| SignalR не підключається       | Перевірити WebSocket підтримку і URL хабу    |
| CORS помилка                   | Увімкнути CORS в `Startup.cs` або `Program.cs` |
| База не оновлюється            | Виконати `dotnet ef migrations`              |

---

## 🧾 Використані джерела / література

- [ASP.NET Core Docs](https://learn.microsoft.com/en-us/aspnet/core)
- [React Docs](https://react.dev)
- [SignalR Docs](https://learn.microsoft.com/en-us/aspnet/core/signalr)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core)
- [Bootstrap Docs](https://getbootstrap.com/docs)
- [PayPal SDK](https://developer.paypal.com/docs/checkout/)
- Stack Overflow, xUnit, Moq

---

