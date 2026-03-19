# Travel Vista

<img width="1914" height="1075" alt="Snimak ekrana 2026-03-19 185823" src="https://github.com/user-attachments/assets/b3825c3b-9cf9-4df9-b3b5-4a228d5dfb1a" />
<img width="1913" height="1077" alt="image" src="https://github.com/user-attachments/assets/3854820e-b62e-4e5d-bb79-573e4a06f4dc" />

---

### Table of Contents

- [Description](#description)
- [Technologies](#technologies)
- [Authors](#authors)

---

## Description

Travel Vista is a full-stack tourism platform for discovering, buying, and experiencing guided tours. The application supports multiple user roles (tourist, author, administrator) and provides both a rich web UI and realtime notifications.

Core experience highlights:
- A role-based interface with navigation menus and protected pages
- Tourist flow: browse tours, view details, manage wishlist, purchase, execute tours, and track history
- Author flow: create and manage tours (including tour problems/steps), handle bundles/coupons/sales, and manage blog content
- Administrator flow: manage platform data such as accounts, equipment, facilities, monuments, encounters, awards, and tour problems
- Realtime notifications using SignalR (updates + notification dropdown + unread badge)
- AI features: chat and text-to-speech (backend routes) integrated into the app UI

---

#### Technologies

- **Frontend**: Angular 16 + Angular Material (UI role-based navigation)
- **Mapping**: Leaflet + routing for the Tourist Map experience
- **Auth**: JWT Bearer token (used for REST calls and SignalR realtime)
- **Realtime**: SignalR notifications with unread badge + dropdown
- **Backend**: ASP.NET Core (.NET 8) built as separate modules (Tours, Stakeholders, Blog, Encounters, Payments)
- **Database**: PostgreSQL with EF Core
- **AI**: Groq-powered chat + ElevenLabs text-to-speech
---

## Authors

- [Vukašin Andrijašević](https://github.com/Vukasin13)
- [Petar Bratić](https://github.com/petarbratic)
- [Anja Guzina](https://github.com/anjaguzina)
- [Luka Stanaćev](https://github.com/stanacev)
- [Petar Šipka](https://github.com/petarsipka)
- [Marko Velimirović](https://github.com/velimirovic)
- [Dragana Vranešević](https://github.com/draganavranesevic)
- [Tamara Savić](https://github.com/savictamara)
- [Jovana Letić](https://github.com/jovanaletic)
- [Anđela Kumazec](https://github.com/andjelakumazec)
- [Tijana Savić](https://github.com/savictijana)
- [Pavle Kapuran](https://github.com/c4ps63)

---

[Back To The Top](#travel-vista)
