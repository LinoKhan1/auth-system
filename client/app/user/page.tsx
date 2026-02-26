"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation"; // App Router v13+
import { useAuth } from "@/features/auth/hooks/useAuth";
import { UserResponse } from "@/features/auth/types/auth.types";

export default function UserDetailsPage({ params }: { params: { id: string } }) {
  const router = useRouter();
  const { fetchUser, getToken } = useAuth(); // useAuth provides a method to get token
  const [user, setUser] = useState<UserResponse | null>(null);

  useEffect(() => {
    const token = getToken(); // get token from hook or localStorage

    // Redirect to login if no token
    if (!token) {
      router.push("/login");
      return;
    }

    fetchUser(params.id)
      .then(setUser)
      .catch(() => router.push("/login"));
  }, [params.id, router, getToken]);

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