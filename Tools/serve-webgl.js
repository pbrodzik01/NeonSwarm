const http = require('http');
const fs = require('fs');
const path = require('path');
const os = require('os');

const root = path.resolve(__dirname, '..', 'Builds', 'WebGL');
const port = Number(process.env.PORT || 8080);
const host = process.argv.includes('--lan') ? '0.0.0.0' : '127.0.0.1';

const types = {
  '.html': 'text/html; charset=utf-8',
  '.js': 'application/javascript',
  '.wasm': 'application/wasm',
  '.data': 'application/octet-stream',
  '.json': 'application/json',
  '.css': 'text/css',
  '.png': 'image/png',
  '.ico': 'image/x-icon',
  '.unityweb': 'application/octet-stream'
};

http.createServer((request, response) => {
  const urlPath = decodeURIComponent(new URL(request.url, 'http://localhost').pathname);
  let file = path.join(root, urlPath === '/' ? 'index.html' : urlPath);
  if (!file.startsWith(root) || !fs.existsSync(file) || fs.statSync(file).isDirectory()) {
    response.writeHead(404);
    response.end('Not found');
    return;
  }
  const headers = { 'Cache-Control': 'no-cache' };
  let extension = path.extname(file);
  if (extension === '.gz') {
    headers['Content-Encoding'] = 'gzip';
    extension = path.extname(file.slice(0, -3));
  }
  headers['Content-Type'] = types[extension] || 'application/octet-stream';
  response.writeHead(200, headers);
  fs.createReadStream(file).pipe(response);
}).listen(port, host, () => {
  console.log(`Neon Swarm WebGL: http://localhost:${port}`);
  if (host === '0.0.0.0') {
    for (const addresses of Object.values(os.networkInterfaces())) {
      for (const address of addresses) {
        if (address.family === 'IPv4' && !address.internal) {
          console.log(`W sieci lokalnej (np. iPhone): http://${address.address}:${port}`);
        }
      }
    }
  }
});
