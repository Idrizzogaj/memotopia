FROM node:12.16.3-slim

WORKDIR /app

ARG API_RELEASE=development

ENV NODE_OPTIONS="--max-old-space-size=1024"

# install dependencies
COPY package.json yarn.lock /app/
RUN yarn install --frozen-lockfile

# copy all other files
COPY . /app

# build the application
RUN yarn build

COPY .env.local /app/dist/

EXPOSE 80

# run the application
# CMD ["node", "-r", "./tsconfig-paths-bootstrap.js", "dist/src/main.js"]
CMD ["yarn", "start:prod"]
