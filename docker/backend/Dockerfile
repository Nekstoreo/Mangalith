# Build stage
FROM eclipse-temurin:25-jdk-alpine AS builder

WORKDIR /build

COPY backend/build.gradle.kts .
COPY backend/settings.gradle.kts .
COPY backend/gradle/ gradle/
COPY backend/gradlew .
COPY backend/src/ src/

RUN chmod +x gradlew && ./gradlew build -x test --no-daemon

# Runtime stage
FROM eclipse-temurin:25-jre-alpine

WORKDIR /app

COPY --from=builder /build/build/libs/*.jar app.jar

EXPOSE 8080

ENTRYPOINT ["java", "-jar", "app.jar"]
