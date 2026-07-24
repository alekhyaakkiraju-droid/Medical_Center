FROM node:22-alpine AS build

WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci --ignore-scripts

COPY . .

ARG API_BASE_URL=http://localhost:8080/api
RUN sed -i "s|http://localhost:5004/api|${API_BASE_URL}|g" src/environments/environment.ts src/environments/environment.development.ts

RUN ./node_modules/.bin/ng build --configuration production

FROM nginx:1-alpine-slim

COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/medical-center/browser /usr/share/nginx/html

RUN chown -R nginx:nginx /usr/share/nginx/html /var/cache/nginx /var/log/nginx /etc/nginx/conf.d

EXPOSE 80

USER nginx

HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
  CMD wget -q --spider http://127.0.0.1/ || exit 1

CMD ["nginx", "-g", "daemon off;"]
