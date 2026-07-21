# Backend documentation

This directory documents the backend as it is implemented in this repository.
It is intentionally a code-level contract, rather than a proposed API.

- [Backend architecture](./architecture.md) explains components, ownership, state, execution paths, deployment, and operational constraints.
- [Client integration and payload reference](./client-data-contract.md) explains every HTTP and Socket.IO message sent to a client, with examples.
- [Professional API and Socket.IO event reference](./api-socket-events.md) provides frontend-oriented schemas, examples, validations, event sequences, and implementation-status notes.

The service is a real-time two-player Tic-Tac-Toe server. Its source of truth is held in the Node.js process; restarting or scaling the process does not preserve or share rooms.
