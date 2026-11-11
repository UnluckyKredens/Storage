export function integerTickAmount(maxValue: number): number | undefined {
  const max = Math.ceil(maxValue);
  return max > 0 && max <= 10 ? max : undefined;
}
