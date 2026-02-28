import { NextRequest, NextResponse } from "next/server";

export function middleware(req: NextRequest) {

  const { pathname } = req.nextUrl;

  const sessionCookie = req.cookies.get("jwt");
  const isAuthenticated = Boolean(sessionCookie);

  const isAuthPage =
    pathname.startsWith("/login") || pathname.startsWith("/register");

  const isProtectedPage = pathname.startsWith("/user");

  if (isProtectedPage && !isAuthenticated) {
    return NextResponse.redirect(new URL("/login", req.url));
  }
  if (isAuthPage && isAuthenticated) {
    return NextResponse.redirect(new URL("/user", req.url));
  }

  return NextResponse.next();

}

export const config = {
  matcher: ["/login", "/register", "/user/:path*"],
};



