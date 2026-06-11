import createMiddleware from "next-intl/middleware";
import { routing } from "./lib/i18n/routing";
import { NextResponse } from "next/server";

const intlMiddleware = createMiddleware(routing);

export default function middleware(request) {
  const { pathname } = request.nextUrl;

  // Extract locale from the path (e.g., /en/register/Login -> locale is "en")
  const segments = pathname.split("/");
  const locale = routing.locales.includes(segments[1]) ? segments[1] : null;
  const localePrefix = locale ? `/${locale}` : "";
  const relativePath = locale ? pathname.substring(localePrefix.length) : pathname;

  // Check access token presence
  const accessToken = request.cookies.get("access_token")?.value;

  const guestPaths = ["/register/Login", "/register/SignUp"];
  const isGuestPath = guestPaths.some(
    (path) => relativePath === path || relativePath.startsWith(path + "/")
  );
  const isProtectedPath =
    relativePath === "/student" || relativePath.startsWith("/student/");

  if (accessToken) {
    if (isGuestPath) {
      const targetLocale = locale || routing.defaultLocale;
      return NextResponse.redirect(
        new URL(`/${targetLocale}/student/homepage`, request.url)
      );
    }
  } else {
    if (isProtectedPath) {
      const targetLocale = locale || routing.defaultLocale;
      return NextResponse.redirect(
        new URL(`/${targetLocale}/register/Login`, request.url)
      );
    }
  }

  return intlMiddleware(request);
}

export const config = {
  // Match all pathnames except for
  // - … if they start with `/api`, `/trpc`, `/_next` or `/_vercel`
  // - … the ones containing a dot (e.g. `favicon.ico`)
  matcher: "/((?!api|trpc|_next|_vercel|.*\\..*).*)",
};
