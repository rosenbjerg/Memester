import { createContext, FunctionalComponent, h } from "preact";
import { Route, Router } from "preact-router";

import NotFoundPage from "../routes/notfound";
import Header from "./Header";
import MemePage from "../routes/meme";
import ThreadPage from "../routes/thread";
import ThreadOverviewPage from "../routes/threads";
import LandingPage from "../routes/landingPage";
import Login from "../routes/login";

// eslint-disable-next-line @typescript-eslint/no-explicit-any
if ((module as any).hot) {
    // tslint:disable-next-line:no-var-requires
    require("preact/debug");
}


const UserContext = createContext('undefined')

const App: FunctionalComponent = () => {
    return (
        <UserContext.Provider value='undefined'>
            <div id="app">
                <Header />
                <Router>
                    <Route path="/" component={LandingPage} />
                    <Route path="/overview" component={ThreadOverviewPage} />
                    <Route path="/:threadId" component={ThreadPage} />
                    <Route path="/:threadId/:memeId" component={MemePage} />
                    <Route path="/login" component={Login} />
                    <NotFoundPage default />
                </Router>
            </div>

        </UserContext.Provider>
    );
};

export default App;
