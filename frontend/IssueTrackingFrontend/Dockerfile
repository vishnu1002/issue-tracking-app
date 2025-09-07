FROM node:20-alpine

WORKDIR /app

COPY package*.json ./

RUN npm ci && npm i -g @angular/cli@20

COPY . .

EXPOSE 4200

CMD ["ng", "serve", "--host", "0.0.0.0", "--port", "4200"]

