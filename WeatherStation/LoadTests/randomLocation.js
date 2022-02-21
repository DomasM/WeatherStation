import http from "k6/http";
import { check } from 'k6';
import { sleep } from "k6";
import { Gauge } from 'k6/metrics';
import { randomString } from 'https://jslib.k6.io/k6-utils/1.1.0/index.js';
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
    http_req_duration: ["p(99)<8000"],
  },
};
export default function() {
    const randomLocation = randomString(12);
    const res = http.get("https://localhost:7019/WeatherConditions/Conditions?location=" + randomLocation);
    check(res, {
        'is status 404': (r) => r.status === 404,
    });
    sleep(0.05);
}
