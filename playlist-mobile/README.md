# playlist-mobile (Ionic + Vue + JavaScript)

Cliente móvil del Taller #2. Reutiliza la misma API de NestJS del Taller #1, sin modificarla.

## Instalar y correr

```bash
npm install
npm run dev
```

Se abre en `http://localhost:8100`.

Asegúrate de tener el backend corriendo antes (`cd backend && npm run start:dev`), ya que esta app le hace peticiones a `http://localhost:3000/api`.

## Compilar para dispositivo/emulador

```bash
npm run build
npx cap add android      # solo la primera vez
npx cap sync android
npx cap open android
```

Revisa `src/services/api.js` si vas a correr en un dispositivo físico: reemplaza la URL fija por la IP de tu PC en la red local.

## Qué se validó antes de entregar este proyecto

El entorno donde armé este proyecto no tiene salida a internet, así que no pude ejecutar un `npm install` real ni levantar `vite`/`ionic serve` en vivo. En su lugar, verifiqué de forma estática:
- Sintaxis válida de todo el JavaScript (`node --check`), incluido el `<script setup>` de cada componente `.vue`.
- JSON válido en `package.json`, `ionic.config.json` y `capacitor.config.json`.
- Balance de etiquetas `<script>`/`<template>` y de cada tag HTML/Ionic dentro del template (nada quedó sin cerrar).
- Que todo componente `Ion*` usado en un template esté importado en su `<script>`.
- Que las rutas del router apunten a archivos que realmente existen.
- Un chequeo con el compilador de TypeScript (`checkJs`) sobre cada componente, usando los paths reales del proyecto, sin errores de lógica (solo quedan, como es normal, los "módulo no encontrado" de paquetes que no están instalados aquí por falta de red — eso se resuelve solo con tu `npm install`).

Lo que **no** pude confirmar: que compile con Vite real y que la UI se vea/funcione en un navegador o emulador de verdad. Si al correr `npm install && npm run dev` te sale algún error, pégamelo tal cual y lo resolvemos.
