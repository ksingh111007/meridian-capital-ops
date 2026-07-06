import type { Metadata } from "next";
import { getNotificationRules } from "@/lib/data";
import { NotificationRulesScreen } from "@/screens/admin/NotificationRulesScreen";

export const metadata: Metadata = { title: "Notification Rules" };

/** Screen 5g — who gets told what, and on which channel. */
export default function NotificationRulesPage() {
  const { rules, channels } = getNotificationRules();
  return <NotificationRulesScreen rules={rules} channels={channels} />;
}
