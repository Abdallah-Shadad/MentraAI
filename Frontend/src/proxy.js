import createMiddleware from "next-intl/middleware";
import { routing } from "./lib/i18n/routing";
import { NextResponse } from "next/server";

const intlMiddleware = createMiddleware(routing);

// Helper to check status with the backend.
const fetchOnboardingStatus = async (cookiesHeader) => {
  const urls = [
    "http://dotnet_api:8080/api/v1",
    process.env.NEXT_PUBLIC_API_URL || "http://localhost:8080/api/v1"
  ];

  for (const baseUrl of urls) {
    try {
      const res = await fetch(`${baseUrl}/onboarding/status`, {
        headers: {
          Cookie: cookiesHeader,
        },
        cache: "no-store",
      });
      
      if (res.status === 401) {
        return { unauthorized: true };
      }
      
      if (res.ok) {
        const json = await res.json();
        return { data: json?.data };
      }
    } catch (e) {
      // fail silently and try the next url
    }
  }
  return null;
};

export default async function proxy(request) {
  const { pathname } = request.nextUrl;

  // Let next-intl handle parsing and updating locale redirects if needed
  const response = intlMiddleware(request);

  // Identify locale
  const segments = pathname.split("/");
  const locale = ["en", "ar"].includes(segments[1]) ? segments[1] : "";
  const relativePath = locale ? pathname.substring(locale.length + 1) || "/" : pathname;

  // Check auth cookies
  const hasAccessToken = request.cookies.has("access_token");
  const hasSession = request.cookies.has("has_session") || hasAccessToken;
  const cookiesHeader = request.headers.get("cookie") || "";

  const isProtectedPath = relativePath === "/student" || relativePath.startsWith("/student/");
  const isOnboardingPath = relativePath === "/onboarding";
  const isAuthPath = relativePath === "/login" || relativePath === "/signup";

  // Case 1: Protected route access
  if (isProtectedPath) {
    if (!hasSession) {
      const redirectUrl = request.nextUrl.clone();
      redirectUrl.pathname = `/${locale || "en"}/login`;
      return NextResponse.redirect(redirectUrl);
    }

    const status = await fetchOnboardingStatus(cookiesHeader);
    if (status?.unauthorized) {
      const redirectUrl = request.nextUrl.clone();
      redirectUrl.pathname = `/${locale || "en"}/login`;
      const res = NextResponse.redirect(redirectUrl);
      res.cookies.delete("access_token");
      res.cookies.delete("refresh_token");
      res.cookies.delete("has_session");
      return res;
    }

    const isOnboarded = status?.data?.isOnboarded === true;
    if (!isOnboarded) {
      const redirectUrl = request.nextUrl.clone();
      redirectUrl.pathname = `/${locale || "en"}/onboarding`;
      return NextResponse.redirect(redirectUrl);
    }
  }

  // Case 2: Onboarding route access
  if (isOnboardingPath) {
    if (!hasSession) {
      const redirectUrl = request.nextUrl.clone();
      redirectUrl.pathname = `/${locale || "en"}/login`;
      return NextResponse.redirect(redirectUrl);
    }

    const status = await fetchOnboardingStatus(cookiesHeader);
    if (status?.unauthorized) {
      const redirectUrl = request.nextUrl.clone();
      redirectUrl.pathname = `/${locale || "en"}/login`;
      const res = NextResponse.redirect(redirectUrl);
      res.cookies.delete("access_token");
      res.cookies.delete("refresh_token");
      res.cookies.delete("has_session");
      return res;
    }

    const isOnboarded = status?.data?.isOnboarded === true;
    if (isOnboarded) {
      const redirectUrl = request.nextUrl.clone();
      redirectUrl.pathname = `/${locale || "en"}/student/homepage`;
      return NextResponse.redirect(redirectUrl);
    }
  }

  // Case 3: Login/Signup route access while logged in
  if (isAuthPath && hasSession) {
    const status = await fetchOnboardingStatus(cookiesHeader);
    if (status && !status.unauthorized) {
      const isOnboarded = status?.data?.isOnboarded === true;
      const redirectUrl = request.nextUrl.clone();
      if (isOnboarded) {
        redirectUrl.pathname = `/${locale || "en"}/student/homepage`;
      } else {
        redirectUrl.pathname = `/${locale || "en"}/onboarding`;
      }
      return NextResponse.redirect(redirectUrl);
    }
  }

  return response;
}

export const config = {
  matcher: [
    // Enable internationalization matchers
    "/",
    "/(ar|en)/:path*",
    // Exclude static files and api routes
    "/((?!api|_next/static|_next/image|assets|Logo|favicon.ico|.*\\..*).*)",
  ],
};
