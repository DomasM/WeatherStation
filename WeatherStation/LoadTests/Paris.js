import http from "k6/http";
import { sleep } from "k6";
import { Gauge } from 'k6/metrics';
export const GaugeContentSize = new Gauge('ContentSize');
export const options = {
  scenarios: {
    read_test: {
      executor: "ramping-vus",
      startVUs: 0,
      stages: [
        { duration: "5s", target: 50 },
        { duration: "10s", target: 50 },
        { duration: "5s", target: 0 },
      ],
      gracefulRampDown: "2s",
    },
    
  },
  thresholds: {
    http_req_duration: ["p(90)<5"],
    http_req_duration: ["p(95)<10"],
    http_req_duration: ["p(99)<250"],
    http_req_failed: ['rate == 0.00'],
    'ContentSize': ['value>300'],
  },
};
export default function() {
    const res = http.get("https://localhost:7019/WeatherConditions/Conditions?location=Paris");
    sleep(0.05);
}
