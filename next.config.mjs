import createNextIntlPlugin from "next-intl/plugin";

const withNextIntl = createNextIntlPlugin("./src/lib/i18n/request.js");

/** @type {import('next').NextConfig} */
const nextConfig = {
  reactCompiler: true,
  // ... أي خيارات ثانية عندك
};

export default withNextIntl(nextConfig);
