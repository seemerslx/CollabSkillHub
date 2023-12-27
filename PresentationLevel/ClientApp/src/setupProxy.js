const {createProxyMiddleware} = require('http-proxy-middleware');
const {env} = require('process');

const target = env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}` :
    env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'http://localhost:21837';

const context = [
    "/api",
];

const onError = (err, req, resp, target) => {
    console.error(`${err.message}`);
}

module.exports = function (app) {
    const appProxy = createProxyMiddleware(context, {
        proxyTimeout: 10000,
        target: target,
        onError: onError,
        secure: false,
        ws: true,
        headers: {
            Connection: 'Keep-Alive'
        }
    });

    app.use(appProxy);
};
