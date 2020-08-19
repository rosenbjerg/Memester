import { FunctionalComponent, h } from "preact";
import { Route, Router } from "preact-router";

import NotFoundPage from "../routes/notfound";
import Header from "./header";
import MemePage from "../routes/meme";
import ThreadPage from "../routes/thread";
import ThreadOverviewPage from "../routes/threads";

// eslint-disable-next-line @typescript-eslint/no-explicit-any
if ((module as any).hot) {
    // tslint:disable-next-line:no-var-requires
    require("preact/debug");
}

const App: FunctionalComponent = () => {
    return (
        <div id="app">
            <Header />
            <Router>
                <Route path="/" component={ThreadOverviewPage} />
                <Route path="/:threadId" component={ThreadPage} />
                <Route path="/:threadId/:memeId" component={MemePage} />
                <NotFoundPage default />
            </Router>
        </div>
    );
};

export default App;
