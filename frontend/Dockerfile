# Frontend: build and serve static assets for docker-compose (single command with API).
FROM node:22-alpine AS build
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM node:22-alpine
WORKDIR /app
RUN npm install -g serve
COPY --from=build /app/dist/frontend/browser /app/dist
EXPOSE 4200
CMD ["serve", "-s", "/app/dist", "-l", "4200"]
