# Project Name

MentarAi

## Tech Stack

- Next.js
- JavaScript
- Tailwind CSS

## Requirements

- Node.js 20+
- npm

## Installation

```bash
npm install
```

## Environment Variables

Create a `.env.local` file:

```env
NEXT_PUBLIC_API_URL=https://mentraai-api.runasp.net/api/v1

```

## Run Development

```bash
npm run dev
```

## Build

```bash
npm run build
```

## Production

```bash
npm run start
```

## Scripts

```json
{
  "dev": "next dev",
  "build": "next build",
  "start": "next start",
  "lint": "next lint"
}
```

## Path Aliases

## The project uses the following alias:

```json
{
  "compilerOptions": {
    "paths": {
      "@/*": ["./src/*"]
    }
  }
}
```

Example:

```js
import Button from "@/components/Button";
```

## Notes

- Uses App Router.
- Uses Tailwind CSS.
- Default language is English.
- Project uses environment variables from .env.local.
