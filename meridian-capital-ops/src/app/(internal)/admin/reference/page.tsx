import type { Metadata } from "next";
import { getReferenceData } from "@/lib/data";
import { ReferenceDataScreen } from "@/screens/admin/ReferenceDataScreen";

export const metadata: Metadata = { title: "Reference Data" };

/** Screen 5e — borrowers, deals, tranches, currencies & calendars. */
export default async function ReferenceDataPage() {
  const { borrowers, currencies, currenciesUpdated, calendars } = await getReferenceData();
  return <ReferenceDataScreen borrowers={borrowers} currencies={currencies} currenciesUpdated={currenciesUpdated} calendars={calendars} />;
}
