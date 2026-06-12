import createNextIntlPlugin from "next-intl/plugin";

const withNextIntl = createNextIntlPlugin("./src/lib/i18n/request.js");

/** @type {import('next').NextConfig} */
const nextConfig = {
  reactCompiler: true,
  // Required for Docker multi-stage production build.
  // Generates .next/standalone — a minimal self-contained server
  // that doesn't need the full node_modules at runtime.
  output: "standalone",
  async rewrites() {
    return [
      {
        source: '/api/v1/:path*',
        destination: 'http://dotnet_api:8080/api/v1/:path*'
      }
    ];
  }
};

export default withNextIntl(nextConfig);
