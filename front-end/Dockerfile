FROM node:22-alpine AS build

WORKDIR /app

COPY package.json package-lock.json .npmrc ./
RUN npm ci --ignore-scripts

COPY . .

ARG API_BASE_URL=/api
ARG ASSET_BASE_URL=
RUN sed -i "s|api: \"/api\"|api: \"${API_BASE_URL}\"|g" src/environments/environment.ts \
    && sed -i "s|assetBaseUrl: \"\"|assetBaseUrl: \"${ASSET_BASE_URL}\"|g" src/environments/environment.ts

RUN ./node_modules/.bin/ng build --configuration production \
    && cp dist/medical-center/browser/index.csr.html dist/medical-center/browser/index.html

FROM nginx:1-alpine-slim

RUN rm -rf /usr/share/nginx/html/*
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/medical-center/browser /usr/share/nginx/html

EXPOSE 80

HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
  CMD wget -q --spider http://127.0.0.1/ || exit 1

CMD ["nginx", "-g", "daemon off;"]
