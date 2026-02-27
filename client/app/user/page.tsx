"use client";
import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import {jwtDecode} from "jwt-decode";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { UserResponse } from "@/features/auth/types/auth.types";

interface JwtPayload {
  sub: string; // assuming token contains user ID in `sub`
}

export default function UserDetailsPage() {
  const router = useRouter();
  const { fetchUser, getToken } = useAuth();
  const [user, setUser] = useState<UserResponse | null>(null);

  useEffect(() => {
    const token = getToken();
    if (!token) {
      router.push("/login");
      return;
    }

    // Decode JWT to get ID
    const { sub: userId } = jwtDecode<JwtPayload>(token);

    fetchUser(userId)
      .then(setUser)
      .catch(() => router.push("/login"));
  }, [fetchUser, getToken, router]);

  if (!user) return <p className="text-center mt-10">Loading user details...</p>;

  return (
    <div className="flex items-center justify-center min-h-screen bg-gray-100">
      <div className="w-full max-w-md p-8 bg-white rounded shadow">
        <h1 className="text-2xl font-bold mb-6 text-center">User Details</h1>
        <div className="space-y-2">
          <p><strong>ID:</strong> {user.id}</p>
          <p><strong>Name:</strong> {user.firstName} {user.lastName}</p>
          <p><strong>Email:</strong> {user.email}</p>
          <p><strong>Created At:</strong> {new Date(user.createdAt).toLocaleString()}</p>
          {user.updatedAt && (
            <p><strong>Updated At:</strong> {new Date(user.updatedAt).toLocaleString()}</p>
          )}
        </div>
      </div>
    </div>
  );
}