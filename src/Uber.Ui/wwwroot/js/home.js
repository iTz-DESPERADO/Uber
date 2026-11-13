(() => {
  const mapElement = document.getElementById('map');
  if (!mapElement || typeof L === 'undefined') return;

  const map = L.map(mapElement).setView([30.0444, 31.2357], 12);
  L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '© OpenStreetMap contributors'
  }).addTo(map);

  const elements = {
    form: document.getElementById('rideForm'),
    pickup: document.getElementById('pickup'),
    dropoff: document.getElementById('dropoff'),
    pickupSuggestions: document.getElementById('pickup-suggestions'),
    dropoffSuggestions: document.getElementById('dropoff-suggestions'),
    message: document.getElementById('locationMessage'),
    mapHint: document.querySelector('#mapHint span'),
    quote: document.getElementById('quote'),
    distance: document.getElementById('distanceText'),
    duration: document.getElementById('durationText'),
    price: document.getElementById('priceText'),
    request: document.getElementById('requestNowBtn'),
    requestStatus: document.getElementById('requestStatus'),
    statusText: document.getElementById('statusText'),
    payment: document.getElementById('selectedPaymentMethod'),
    paymentInfo: document.getElementById('paymentInfo'),
    wallet: document.getElementById('walletBalance'),
    balance: document.getElementById('balanceAmount')
  };

  const state = {
    pickup: null,
    dropoff: null,
    pickupMarker: null,
    dropoffMarker: null,
    route: null,
    distance: 0,
    duration: 0,
    price: 0
  };

  function setMessage(message, visible = true) {
    elements.message.textContent = message;
    elements.message.hidden = !visible;
  }

  function setMapHint(message) {
    elements.mapHint.textContent = message;
  }

  function setSuggestionState(container, message) {
    container.innerHTML = '';
    const stateElement = document.createElement('div');
    stateElement.className = 'suggestion-state';
    stateElement.textContent = message;
    container.appendChild(stateElement);
  }

  async function searchLocations(query, signal) {
    const response = await fetch(`/api/locations/search?query=${encodeURIComponent(query)}`, { signal });
    if (!response.ok) throw new Error('Location search failed');
    return response.json();
  }

  function updateMarker(kind, place) {
    const coordinates = [place.latitude, place.longitude];
    const markerKey = `${kind}Marker`;
    if (state[markerKey]) map.removeLayer(state[markerKey]);
    state[kind] = coordinates;
    state[markerKey] = L.marker(coordinates).addTo(map)
      .bindPopup(kind === 'pickup' ? 'Pickup' : 'Destination');
    map.setView(coordinates, 14);
    setMapHint(kind === 'pickup' && !state.dropoff
      ? 'Pickup selected. Choose your destination.'
      : 'Destination selected. Choose your pickup location.');
    drawRouteWhenReady();
  }

  function renderSuggestions(container, input, places, kind) {
    container.replaceChildren();
    if (!places.length) {
      setSuggestionState(container, 'No matching places found. Try a nearby landmark or area.');
      return;
    }

    places.forEach(place => {
      const button = document.createElement('button');
      button.type = 'button';
      button.className = 'suggestion-item';
      button.setAttribute('role', 'option');
      const icon = document.createElement('i');
      icon.className = 'fa-solid fa-location-dot';
      const label = document.createElement('span');
      label.textContent = place.displayName;
      button.append(icon, label);
      button.addEventListener('click', () => {
        input.value = place.displayName;
        container.replaceChildren();
        updateMarker(kind, place);
      });
      container.appendChild(button);
    });
  }

  function attachAutocomplete(input, container, kind) {
    let timer;
    let controller;

    input.addEventListener('input', () => {
      state[kind] = null;
      elements.request.disabled = true;
      elements.quote.hidden = true;
      clearTimeout(timer);
      controller?.abort();
      const query = input.value.trim();

      if (query.length < 2) {
        container.replaceChildren();
        return;
      }

      setSuggestionState(container, 'Searching locations…');
      timer = setTimeout(async () => {
        controller = new AbortController();
        try {
          const places = await searchLocations(query, controller.signal);
          renderSuggestions(container, input, places, kind);
        } catch (error) {
          if (error.name !== 'AbortError') {
            setSuggestionState(container, 'Location search is unavailable. Please try again.');
          }
        }
      }, 350);
    });

    input.addEventListener('focus', () => {
      if (input.value.trim().length >= 2 && !state[kind]) input.dispatchEvent(new Event('input'));
    });
  }

  async function resolveLocation(input, kind) {
    if (state[kind]) return state[kind];
    const places = await searchLocations(input.value.trim());
    if (!places.length) return null;
    input.value = places[0].displayName;
    updateMarker(kind, places[0]);
    return state[kind];
  }

  async function getRoute(start, end) {
    const url = `https://router.project-osrm.org/route/v1/driving/${start[1]},${start[0]};${end[1]},${end[0]}?overview=full&geometries=geojson`;
    const response = await fetch(url);
    if (!response.ok) throw new Error('Route service failed');
    const data = await response.json();
    if (!data.routes?.length) throw new Error('No route found');
    const route = data.routes[0];
    return {
      path: route.geometry.coordinates.map(coordinate => [coordinate[1], coordinate[0]]),
      distance: Number((route.distance / 1000).toFixed(2)),
      duration: Number((route.duration / 60).toFixed(1))
    };
  }

  async function drawRouteWhenReady() {
    if (!state.pickup || !state.dropoff) return;
    try {
      setMessage('Calculating the best route…');
      const result = await getRoute(state.pickup, state.dropoff);
      if (state.route) map.removeLayer(state.route);
      state.route = L.polyline(result.path, { color: '#000', weight: 5, opacity: .8 }).addTo(map);
      map.fitBounds(state.route.getBounds(), { padding: [35, 35] });
      state.distance = result.distance;
      state.duration = result.duration;
      state.price = (result.distance * 7) + (result.duration * 1.5) + 7;
      elements.distance.textContent = result.distance.toFixed(2);
      elements.duration.textContent = result.duration.toFixed(1);
      elements.price.textContent = state.price.toFixed(2);
      elements.quote.hidden = false;
      elements.request.disabled = false;
      setMapHint(`Route ready: ${result.distance.toFixed(2)} km, about ${result.duration.toFixed(0)} minutes.`);
      setMessage('', false);
    } catch {
      setMapHint('The route could not be displayed. Try more specific locations.');
      setMessage('We could not calculate this route. Try more specific locations.');
    }
  }

  elements.form.addEventListener('submit', async event => {
    event.preventDefault();
    if (!elements.pickup.value.trim() || !elements.dropoff.value.trim()) {
      setMessage('Enter both pickup and destination locations.');
      return;
    }

    try {
      setMessage('Confirming your locations…');
      const [pickup, dropoff] = await Promise.all([
        resolveLocation(elements.pickup, 'pickup'),
        resolveLocation(elements.dropoff, 'dropoff')
      ]);
      if (!pickup || !dropoff) {
        setMessage('Select valid locations from the suggestions.');
        return;
      }
      await drawRouteWhenReady();
    } catch {
      setMessage('Location search is unavailable right now. Please try again.');
    }
  });

  async function getUserBalance() {
    const response = await fetch('/api/users/me', { credentials: 'same-origin' });
    if (!response.ok) return null;
    const data = await response.json();
    return Number(data.customer?.balance ?? 0);
  }

  async function updatePayment(method) {
    elements.payment.textContent = method;
    const walletSelected = method === 'Wallet';
    elements.wallet.hidden = !walletSelected;
    elements.paymentInfo.textContent = walletSelected
      ? 'Payment will be deducted from your wallet after the ride is completed.'
      : 'Payment will be collected in cash after the ride.';

    if (walletSelected) {
      const balance = await getUserBalance();
      elements.balance.textContent = balance === null ? '—' : balance.toFixed(2);
    }
  }

  document.querySelectorAll('input[name="paymentMethod"]').forEach(input => {
    input.addEventListener('change', () => updatePayment(input.value));
  });

  elements.request.addEventListener('click', async () => {
    const balance = await getUserBalance();
    if (balance === null) {
      window.location.href = '/Account/Login';
      return;
    }

    const paymentMethod = document.querySelector('input[name="paymentMethod"]:checked').value;
    if (paymentMethod === 'Wallet' && balance < state.price) {
      setMessage(`Your wallet balance is EGP ${balance.toFixed(2)}. Add funds or choose cash.`);
      return;
    }

    elements.request.disabled = true;
    elements.requestStatus.hidden = false;
    elements.statusText.textContent = 'Finding a nearby driver…';

    try {
      const response = await fetch('/api/rides', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          startLat: state.pickup[0],
          startLng: state.pickup[1],
          endLat: state.dropoff[0],
          endLng: state.dropoff[1],
          distance: state.distance,
          duration: state.duration,
          paymentMethod
        })
      });

      const data = await response.json();
      if (!response.ok) throw new Error(data.detail || data.message || 'Could not request the ride.');
      elements.statusText.textContent = 'Ride requested. Redirecting…';
      window.setTimeout(() => {
        window.location.href = data.rideId ? `/Ride/WaitingForDriver/${data.rideId}` : '/Ride/WaitingForDriver';
      }, 700);
    } catch (error) {
      elements.request.disabled = false;
      elements.requestStatus.hidden = true;
      setMessage(error.message || 'Could not request the ride. Please try again.');
    }
  });

  document.addEventListener('click', event => {
    if (!elements.pickup.contains(event.target) && !elements.pickupSuggestions.contains(event.target)) {
      elements.pickupSuggestions.replaceChildren();
    }
    if (!elements.dropoff.contains(event.target) && !elements.dropoffSuggestions.contains(event.target)) {
      elements.dropoffSuggestions.replaceChildren();
    }
  });

  attachAutocomplete(elements.pickup, elements.pickupSuggestions, 'pickup');
  attachAutocomplete(elements.dropoff, elements.dropoffSuggestions, 'dropoff');
  updatePayment('Wallet');
})();
