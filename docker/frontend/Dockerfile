# Build stage
FROM node:24-alpine AS builder

WORKDIR /build

COPY frontend/package.json frontend/pnpm-lock.yaml ./

RUN npm install -g pnpm && pnpm install --frozen-lockfile

COPY frontend/ .

RUN pnpm build

# Runtime stage
FROM node:24-alpine

WORKDIR /app

ENV NODE_ENV=production

COPY --from=builder /build/package.json /build/pnpm-lock.yaml ./
COPY --from=builder /build/.next ./.next
COPY --from=builder /build/public ./public

RUN npm install -g pnpm && pnpm install --prod --frozen-lockfile

EXPOSE 3000

CMD ["pnpm", "start"]
