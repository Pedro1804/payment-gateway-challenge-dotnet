# Payments API — request examples and expected behavior

## Running the demo

```bash
docker compose up --build -d
```

- Gateway: `http://localhost:5067` — Swagger UI: `http://localhost:5067/swagger`
- Bank simulator (Mountebank): `http://localhost:8080`

The simulator decides on the card's **last digit**: odd → authorized, even → declined, `0` → `503`.

Base body (amount in minor units: `1050` = 10.50 GBP):

```json
{
  "cardNumber": "2222405343248877",
  "expiryMonth": 4,
  "expiryYear": 2027,
  "currency": "GBP",
  "amount": 1050,
  "cvv": "123"
}
```

## Overview

| Scenario | Request | Response | Bank called |
|---|---|---|---|
| Authorized card | odd last digit | `201` `status: "Authorized"` + `Location` | yes |
| Declined card | even last digit | `201` `status: "Declined"` + `Location` | yes |
| Bank unavailable | last digit `0` | `502` "Acquiring bank unavailable" | yes |
| Invalid field (domain) | e.g. `cardNumber: "1234"` | `400` `paymentStatus: "Rejected"`, first error only | no |
| Missing / `null` / wrongly typed field | e.g. no `cvv`, `cvv: null`, `amount: 10.5` | `400` `paymentStatus: "Rejected"` | no |
| Retrieve a processed payment | `GET /api/payments/{id}` | `200` with the same body as the `POST` | no |
| Retrieve an unknown payment | unknown id or not a GUID | `404` | no |

## 1. Authorized payment — 201

```bash
curl -i -X POST http://localhost:5067/api/payments -H 'Content-Type: application/json' \
  -d '{"cardNumber":"2222405343248877","expiryMonth":4,"expiryYear":2027,"currency":"GBP","amount":1050,"cvv":"123"}'
```

```http
HTTP/1.1 201 Created
Location: http://localhost:5067/api/payments/ab2d1146-a346-46da-80e2-d02aa5deca4d

{"id":"ab2d1146-a346-46da-80e2-d02aa5deca4d","status":"Authorized","cardNumberLastFour":"8877",
 "expiryMonth":4,"expiryYear":2027,"currency":"GBP","amount":1050}
```

- Only the last four digits are returned, as a **string**; never the CVV.
- `status` is serialized as a string, not a number.

## 2. Declined payment — 201

Same request with `"cardNumber":"2222405343248112"`:

```json
{"id":"9738df90-…","status":"Declined","cardNumberLastFour":"8112","expiryMonth":4,"expiryYear":2027,"currency":"GBP","amount":1050}
```

A bank decline is a business outcome: the payment is created (`201`).

## 3. Bank unavailable — 502

Same request with `"cardNumber":"2222405343248110"`:

```http
HTTP/1.1 502 Bad Gateway
Content-Type: application/problem+json

{"type":"https://tools.ietf.org/html/rfc9110#section-15.6.3","title":"Acquiring bank unavailable","status":502,"traceId":"…"}
```

Any non-200 bank response, a timeout or a network error yields the same `502`. The payment is not stored.

## 4. Invalid request (domain validation) — 400 Rejected

Invalid `cardNumber` and `currency`; only the **first** error is returned (order: cardNumber, expiry, currency, amount, cvv):

```bash
curl -i -X POST http://localhost:5067/api/payments -H 'Content-Type: application/json' \
  -d '{"cardNumber":"1234","expiryMonth":4,"expiryYear":2027,"currency":"JPY","amount":1050,"cvv":"123"}'
```

```http
HTTP/1.1 400 Bad Request
Content-Type: application/problem+json

{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,
 "errors":{"cardNumber":["Card number must contain between 14 and 19 digits."]},
 "traceId":"…","paymentStatus":"Rejected"}
```

Other domain rules (same format):

| Field | Rule | Rejected example | Accepted edge case |
|---|---|---|---|
| `cardNumber` | 14 to 19 digits | `"1234"`, `"2222-4053-4324-8877"` | |
| `expiryMonth` | 1 to 12 | `13` | |
| `expiryYear` | month/year not in the past | the previous month | the current month |
| `currency` | `EUR`, `USD` or `GBP` | `"JPY"`, `"gbp"` (case-sensitive) | |
| `amount` | strictly positive integer | `0` | |
| `cvv` | 3 or 4 digits | `"12"` | |

## 5. Missing, `null` or wrongly typed field — 400 Rejected

Rejected by ASP.NET before reaching the domain, in the **same format** (`paymentStatus: "Rejected"`):

| Request | `errors` returned |
|---|---|
| no `cvv` | `"$"`: "JSON deserialization for type 'PaymentGateway.Api.Api.Requests.PostPaymentRequest' was missing required properties, including the following: cvv", `"request"`: "The request field is required." |
| `"cvv": null` | `"Cvv"`: "The Cvv field is required." |
| `"amount": 10.5` | `"$.amount"`: "The JSON value could not be converted to System.Int32. …", `"request"`: "The request field is required." |
| `"cvv": 123` | `"$.cvv"`: "The JSON value could not be converted to System.String. …", `"request"`: "The request field is required." |

## 6. Retrieving a payment — GET

Using the `id` (or the `Location` header) of a processed payment, here with card `2222405343240123`:

```bash
curl -i http://localhost:5067/api/payments/00f5d4ae-f4ee-4976-bf80-f929bee3f78c
```

```http
HTTP/1.1 200 OK

{"id":"00f5d4ae-f4ee-4976-bf80-f929bee3f78c","status":"Authorized","cardNumberLastFour":"0123",
 "expiryMonth":4,"expiryYear":2027,"currency":"GBP","amount":1050}
```

- The body is identical to the `POST` response.
- `cardNumberLastFour` keeps its leading zero.

| Request | Response |
|---|---|
| unknown GUID | `404` `application/problem+json` |
| id that is not a GUID (e.g. `/api/payments/42`) | `404` with an empty body |
| id of a rejected request | impossible: a rejection returns no `id` |

## Logging

- Nothing sensitive is exposed: neither the full card number nor the CVV, in responses or in logs (`docker logs payment_gateway`).
- Rejections are logged at `Information` with the field **name** only ("Payment rejected because of an invalid cardNumber").
- Processed payments are logged with their id and status ("Payment 00f5d4ae-… processed with status Authorized").
