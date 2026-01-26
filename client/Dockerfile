# ============================================
# TaskFlow Client Dockerfile
# ============================================
# Multi-stage build: Node for building, nginx for serving

# Stage 1: Build the React application
FROM node:20-alpine AS build
WORKDIR /app

# Copy package files for dependency caching
COPY package.json package-lock.json ./

# Install dependencies
RUN npm ci

# Copy source code
COPY . .

# Build with empty VITE_API_URL so requests use relative paths (proxied by nginx)
RUN VITE_API_URL= npm run build

# Stage 2: Serve with nginx
FROM nginx:alpine AS runtime

# Copy custom nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy built assets from build stage
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
